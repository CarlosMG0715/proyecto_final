using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Services.Dtos;
using Granja.Tests.Support;

namespace Granja.Tests;

public class VencimientoYReportesTests
{
    [Fact]
    public async Task Al_procesar_vencimientos_la_cantidad_disponible_pasa_a_danada_y_el_reporte_la_refleja()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync();
        await f.VenderAsync(3, TipoProducto.Yogurt, 20); // quedan 30 L en Y1

        var proceso = await f.Inventario.ProcesarVencimientosAsync(GranjaFixture.Dia(17));

        Assert.Equal(1, proceso.LotesProcesados);
        Assert.Equal(30m, proceso.Perdidas[0].CantidadDanada);

        var danado = await f.Reportes.ProductoDanadoAsync(null, null);
        var lote = Assert.Single(danado.Lotes);
        Assert.Equal(TipoProducto.Yogurt, lote.Tipo);
        Assert.Equal(30m, lote.CantidadDanada);
        Assert.Equal(GranjaFixture.Dia(16), lote.FechaVencimiento);
        Assert.Equal(30m, Assert.Single(danado.Totales).Cantidad);
    }

    [Fact]
    public async Task Procesar_vencimientos_dos_veces_no_duplica_el_producto_danado()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync();

        await f.Inventario.ProcesarVencimientosAsync(GranjaFixture.Dia(17));
        var segunda = await f.Inventario.ProcesarVencimientosAsync(GranjaFixture.Dia(17));

        Assert.Equal(0, segunda.LotesProcesados);
        var total = await f.Reportes.ProductoTotalAsync();
        Assert.Equal(50m, total.Productos.Single(p => p.Tipo == TipoProducto.Yogurt).Danado);
    }

    [Fact]
    public async Task Escenario_de_referencia_completo()
    {
        using var f = new GranjaFixture();

        // Días 1 y 2: producción y transformación.
        await f.CargarProduccionDeReferenciaAsync();

        // Día 3: ventas.
        var queso = await f.VenderAsync(3, TipoProducto.Queso, 12);
        Assert.Equal(new[] { 10m, 2m }, queso.Detalle.Select(d => d.Cantidad).ToArray());
        await f.VenderAsync(3, TipoProducto.Yogurt, 20);

        // Día 17: vencimientos y venta rechazada.
        await f.Inventario.ProcesarVencimientosAsync(GranjaFixture.Dia(17));
        await Assert.ThrowsAsync<InventarioInsuficienteException>(() => f.VenderAsync(17, TipoProducto.Yogurt, 5));

        // Reporte de producto total.
        var total = await f.Reportes.ProductoTotalAsync();
        var q = total.Productos.Single(p => p.Tipo == TipoProducto.Queso);
        var y = total.Productos.Single(p => p.Tipo == TipoProducto.Yogurt);
        Assert.Equal((25m, 12m, 13m, 0m), (q.Producido, q.Vendido, q.Disponible, q.Danado));
        Assert.Equal((50m, 20m, 0m, 30m), (y.Producido, y.Vendido, y.Disponible, y.Danado));

        // Reporte de leche por día: 150 L el día 1 y 150 L el día 2.
        var leche = await f.Reportes.LecheDiariaAsync(null, null);
        Assert.Equal(2, leche.Dias.Count);
        Assert.All(leche.Dias, d => Assert.Equal(150m, d.LitrosProducidos));

        // Reporte de ventas: 12 kg × 28.000 + 20 L × 6.000.
        var ventas = await f.Reportes.VentasAsync(null, null, null);
        Assert.Equal(2, ventas.Ventas.Count);
        Assert.Equal(336000m + 120000m, ventas.TotalGeneral);

        // Reporte de producto por día.
        var diario = await f.Reportes.ProductoDiarioAsync(null, null);
        Assert.Equal(3, diario.Items.Count); // día 1 queso, día 1 yogurt, día 2 queso
    }

    [Fact]
    public async Task Reporte_de_ventas_filtra_por_rango_y_tipo()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync();
        await f.VenderAsync(3, TipoProducto.Queso, 5);
        await f.VenderAsync(4, TipoProducto.Yogurt, 10);

        var soloQueso = await f.Reportes.VentasAsync(null, null, TipoProducto.Queso);
        var soloDia4 = await f.Reportes.VentasAsync(GranjaFixture.Dia(4), GranjaFixture.Dia(4), null);

        Assert.Single(soloQueso.Ventas);
        Assert.Equal(140000m, soloQueso.TotalGeneral);
        Assert.Equal(TipoProducto.Yogurt, Assert.Single(soloDia4.Ventas).TipoProducto);
        await Assert.ThrowsAsync<ValidacionException>(() =>
            f.Reportes.VentasAsync(GranjaFixture.Dia(5), GranjaFixture.Dia(1), null));
    }

    [Fact]
    public async Task Reporte_de_vacas_calcula_promedio_maxima_minima_y_las_que_estan_bajo_el_promedio()
    {
        using var f = new GranjaFixture();
        await f.Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest
        {
            Fecha = GranjaFixture.Dia(1),
            Vacas = new List<ProduccionVacaRequest>
            {
                new() { VacaId = 1, Litros = 20 },
                new() { VacaId = 2, Litros = 10 }
            }
        }); // las otras 8 vacas producen l = 15 L → total 150 L

        var reporte = await f.Reportes.VacasAsync(null, null);

        Assert.Equal(15m, reporte.PromedioPorVaca);
        Assert.Equal(1, reporte.MasProductora!.VacaId);
        Assert.Equal(2, reporte.MenosProductora!.VacaId);
        Assert.Equal(2, Assert.Single(reporte.BajoElPromedio).VacaId);
        Assert.Equal(10, reporte.Vacas.Count);
    }

    [Fact]
    public async Task Por_vencer_lista_solo_lotes_con_existencias_dentro_de_la_ventana()
    {
        using var f = new GranjaFixture();
        await f.CargarProduccionDeReferenciaAsync(); // Y1 vence el día 16; Q1 y Q2 vencen el 31 y el 32

        var porVencer = await f.Inventario.ObtenerPorVencerAsync(3, GranjaFixture.Dia(14));

        var lote = Assert.Single(porVencer);
        Assert.Equal(TipoProducto.Yogurt, lote.Tipo);
        Assert.Equal(2, lote.DiasParaVencer);
    }

    [Fact]
    public async Task Actualizar_n_sincroniza_las_vacas_activas()
    {
        using var f = new GranjaFixture();

        await f.Config.ActualizarAsync(new ActualizarGranjaRequest { NumeroVacas = 12, LitrosPorVaca = 15 });
        Assert.Equal(12, (await f.Config.ListarVacasAsync()).Count(v => v.Activa));

        await f.Config.ActualizarAsync(new ActualizarGranjaRequest { NumeroVacas = 8, LitrosPorVaca = 15 });
        Assert.Equal(8, (await f.Config.ListarVacasAsync()).Count(v => v.Activa));
    }
}
