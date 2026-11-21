using AutoMapper;
using Granja.Domain.Enums;
using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Granja.Infrastructure.Repositories;
using Granja.Services;
using Granja.Services.Dtos;
using Granja.Services.Mappings;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace Granja.Tests.Support;

public sealed class FakeClock : IClock
{
    public DateOnly Today { get; set; } = new(2026, 12, 1);
}

/// <summary>
/// Arma los servicios reales sobre una base SQLite en memoria (con los datos iniciales n = 10, l = 15 L).
/// Cada prueba usa su propia base, así que son independientes entre sí.
/// </summary>
public sealed class GranjaFixture : IDisposable
{
    private readonly SqliteConnection _connection;

    public GranjaDbContext Db { get; }
    public FakeClock Clock { get; } = new();
    public IGranjaService Config { get; }
    public IProduccionService Produccion { get; }
    public IInventarioService Inventario { get; }
    public IVentaService Ventas { get; }
    public IReporteService Reportes { get; }

    public GranjaFixture()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<GranjaDbContext>().UseSqlite(_connection).Options;
        Db = new GranjaDbContext(options);
        Db.Database.EnsureCreated();

        var mapper = new MapperConfiguration(c => c.AddProfile(new MappingProfile()), NullLoggerFactory.Instance).CreateMapper();

        var granjas = new GranjaRepository(Db);
        var vacas = new VacaRepository(Db);
        var producciones = new ProduccionRepository(Db);
        var lotes = new LoteRepository(Db);
        var ventas = new VentaRepository(Db);
        var uow = new UnitOfWork(Db);

        Config = new GranjaService(granjas, vacas, uow, mapper);
        Produccion = new ProduccionService(granjas, vacas, producciones, lotes, uow, Clock, mapper);
        Inventario = new InventarioService(lotes, uow, Clock, mapper);
        Ventas = new VentaService(granjas, lotes, ventas, uow, Clock, mapper);
        Reportes = new ReporteService(ventas, lotes, producciones, vacas);
    }

    public static DateOnly Dia(int dia) => new(2026, 12, dia);

    public Task<ProduccionDiaResponse> ProducirAsync(int dia) =>
        Produccion.RegistrarProduccionAsync(new RegistrarProduccionRequest { Fecha = Dia(dia) });

    public Task<TransformacionResponse> TransformarAsync(int dia, TipoProducto tipo, decimal litros) =>
        Produccion.TransformarAsync(new TransformarLecheRequest { Fecha = Dia(dia), TipoProducto = tipo, Litros = litros });

    public Task<VentaResponse> VenderAsync(int dia, TipoProducto tipo, decimal cantidad) =>
        Ventas.RegistrarVentaAsync(new VentaRequest { Fecha = Dia(dia), TipoProducto = tipo, Cantidad = cantidad });

    /// <summary>Días 1 y 2 del escenario de referencia: Q1 = 10 kg, Y1 = 50 L, Q2 = 15 kg.</summary>
    public async Task CargarProduccionDeReferenciaAsync()
    {
        await ProducirAsync(1);
        await TransformarAsync(1, TipoProducto.Queso, 100);
        await TransformarAsync(1, TipoProducto.Yogurt, 50);

        await ProducirAsync(2);
        await TransformarAsync(2, TipoProducto.Queso, 150);
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
