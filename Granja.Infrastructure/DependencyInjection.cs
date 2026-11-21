using Granja.Domain.Interfaces;
using Granja.Infrastructure.Data;
using Granja.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Granja.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddRepositories(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection") ?? "Data Source=granja.db";

        services.AddDbContext<GranjaDbContext>(options => options.UseSqlite(connectionString));

        // aquí van los repositorios
        services.AddScoped<IGranjaRepository, GranjaRepository>();
        services.AddScoped<IVacaRepository, VacaRepository>();
        services.AddScoped<IProduccionRepository, ProduccionRepository>();
        services.AddScoped<ILoteRepository, LoteRepository>();
        services.AddScoped<IVentaRepository, VentaRepository>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddSingleton<IClock, SystemClock>();

        return services;
    }
}
