using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Tests.Support;

namespace Granja.Tests;

public class VentaTests
{
    [Fact]
    public async Task Una_venta_toma_producto_del_lote_mas_antiguo_antes_que_del_mas_nuevo()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync(); // Q1 (día 1) = 10 kg, Q2 (día 2) = 15 kg

        var venta = await f.VenderAsync(3, TipoProducto.Queso, 5);

        var detalle = Assert.Single(venta.Detalle);
        Assert.Equal(GranjaFixture.Dia(1), detalle.FechaProduccion);
        Assert.Equal(5m, detalle.Cantidad);
    }

    [Fact]
    public async Task Una_venta_que_excede_un_lote_consume_el_siguiente_y_genera_varios_detalles()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync();

        var venta = await f.VenderAsync(3, TipoProducto.Queso, 12);

        Assert.Equal(2, venta.Detalle.Count);
        Assert.Equal(1, venta.Detalle[0].LoteId);
        Assert.Equal(10m, venta.Detalle[0].Cantidad);
        Assert.Equal(3, venta.Detalle[1].LoteId);
        Assert.Equal(2m, venta.Detalle[1].Cantidad);
        Assert.Equal(12m, venta.Cantidad);
        Assert.Equal(28000m, venta.PrecioUnitario);
        Assert.Equal(336000m, venta.Total);
    }

    [Fact]
    public async Task Un_lote_vencido_nunca_se_vende_aunque_sea_el_mas_antiguo()
    {
        using var f = new GranjaFixture();

        await f.ProducirAsync(1);
        await f.TransformarAsync(1, TipoProducto.Yogurt, 50);  // Y1: vence el día 16
        await f.ProducirAsync(10);
        await f.TransformarAsync(10, TipoProducto.Yogurt, 40); // Y2: vence el día 25

        // Día 17: Y1 está vencido; solo se puede vender de Y2.
        var venta = await f.VenderAsync(17, TipoProducto.Yogurt, 10);

        var detalle = Assert.Single(venta.Detalle);
        Assert.Equal(GranjaFixture.Dia(10), detalle.FechaProduccion);

        // Y1 sigue sin ser vendible aunque tenga 50 L: con 60 L pedidos no alcanza.
        await Assert.ThrowsAsync<InventarioInsuficienteException>(() => f.VenderAsync(17, TipoProducto.Yogurt, 60));
    }

    [Fact]
    public async Task El_ultimo_dia_de_vida_util_el_lote_todavia_se_puede_vender()
    {
        using var f = new GranjaFixture();
        await f.ProducirAsync(1);
        await f.TransformarAsync(1, TipoProducto.Yogurt, 50); // vence el día 16

        var venta = await f.VenderAsync(16, TipoProducto.Yogurt, 5);

        Assert.Equal(5m, venta.Cantidad);
    }

    [Fact]
    public async Task Una_venta_con_existencias_insuficientes_no_modifica_el_inventario()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync(); // 25 kg de queso en total

        await Assert.ThrowsAsync<InventarioInsuficienteException>(() => f.VenderAsync(3, TipoProducto.Queso, 26));

        f.Db.ChangeTracker.Clear();
        var inventario = await f.Inventario.ObtenerInventarioAsync(GranjaFixture.Dia(3), TipoProducto.Queso);
        Assert.Equal(25m, inventario.Lotes.Sum(l => l.CantidadDisponible));
        Assert.Empty(await f.Ventas.ListarAsync(null, null, null));
    }

    [Fact]
    public async Task La_cantidad_de_la_venta_debe_ser_mayor_que_cero()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync();

        await Assert.ThrowsAsync<ValidacionException>(() => f.VenderAsync(3, TipoProducto.Queso, 0));
    }

    [Fact]
    public async Task Se_puede_consultar_una_venta_registrada_y_una_inexistente_da_404()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync();
        var venta = await f.VenderAsync(3, TipoProducto.Queso, 12);

        var consultada = await f.Ventas.ObtenerAsync(venta.Id);

        Assert.Equal(2, consultada.Detalle.Count);
        await Assert.ThrowsAsync<EntidadNoEncontradaException>(() => f.Ventas.ObtenerAsync(999));
    }
}
