using Granja.Domain.Enums;
using Granja.Domain.Exceptions;
using Granja.Services.Dtos;
using Granja.Tests.Support;

namespace Granja.Tests;

public class ProduccionTests
{
    [Fact]
    public async Task La_produccion_diaria_es_n_por_l()
    {
        using var f = new GranjaFixture();

        var produccion = await f.ProducirAsync(1);

        Assert.Equal(10, produccion.NumeroVacas);
        Assert.Equal(150m, produccion.TotalLitros); // 10 vacas × 15 L
    }

    [Fact]
    public async Task La_produccion_se_adapta_a_n_y_l_configurados()
    {
        using var f = new GranjaFixture();
        await f.Config.ActualizarAsync(new ActualizarGranjaRequest { NumeroVacas = 4, LitrosPorVaca = 20 });

        var produccion = await f.ProducirAsync(1);

        Assert.Equal(4, produccion.NumeroVacas);
        Assert.Equal(80m, produccion.TotalLitros);
    }

    [Fact]
    public async Task La_produccion_no_puede_registrarse_dos_veces_para_la_misma_fecha()
    {
        using var f = new GranjaFixture();
        await f.ProducirAsync(1);

        await Assert.ThrowsAsync<ProduccionYaRegistradaException>(() => f.ProducirAsync(1));
    }

    [Fact]
    public async Task No_se_pueden_transformar_mas_litros_que_los_disponibles_del_dia()
    {
        using var f = new GranjaFixture();
        await f.ProducirAsync(1);
        await f.TransformarAsync(1, TipoProducto.Queso, 100);
        await f.TransformarAsync(1, TipoProducto.Yogurt, 50);

        // Ya se transformaron los 150 L del día: no queda leche.
        await Assert.ThrowsAsync<LecheInsuficienteException>(() => f.TransformarAsync(1, TipoProducto.Queso, 1));
    }

    [Fact]
    public async Task No_se_puede_transformar_leche_de_un_dia_sin_produccion()
    {
        using var f = new GranjaFixture();

        await Assert.ThrowsAsync<LecheInsuficienteException>(() => f.TransformarAsync(5, TipoProducto.Yogurt, 10));
    }

    [Fact]
    public async Task La_transformacion_genera_un_lote_con_cantidad_y_vencimiento_segun_los_parametros()
    {
        using var f = new GranjaFixture();
        await f.ProducirAsync(1);

        var queso = await f.TransformarAsync(1, TipoProducto.Queso, 100);
        var yogurt = await f.TransformarAsync(1, TipoProducto.Yogurt, 50);

        Assert.Equal(10m, queso.Lote.CantidadInicial);
        Assert.Equal(GranjaFixture.Dia(31), queso.Lote.FechaVencimiento); // 1 dic + 30 días
        Assert.Equal(50m, yogurt.Lote.CantidadInicial);
        Assert.Equal(GranjaFixture.Dia(16), yogurt.Lote.FechaVencimiento); // 1 dic + 15 días
    }

    [Fact]
    public async Task La_leche_no_transformada_se_reporta_como_no_aprovechada()
    {
        using var f = new GranjaFixture();
        await f.ProducirAsync(1);
        await f.TransformarAsync(1, TipoProducto.Queso, 100);

        var reporte = await f.Reportes.LecheDiariaAsync(null, null);

        var dia = Assert.Single(reporte.Dias);
        Assert.Equal(150m, dia.LitrosProducidos);
        Assert.Equal(100m, dia.LitrosTransformados);
        Assert.Equal(50m, dia.LitrosNoAprovechados);
    }
}
