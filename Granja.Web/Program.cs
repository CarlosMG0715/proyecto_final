using System.Text.Json.Serialization;
using AutoMapper;
using Granja.Infrastructure;
using Granja.Infrastructure.Data;
using Granja.Services;
using Granja.Services.Mappings;
using Granja.Web.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.OpenApi.Models;

namespace Granja.Web;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var configuration = builder.Configuration;

        // Add services to the container (raíz de composición)
        builder.Services.AddServices();
        builder.Services.AddRepositories(configuration);

        builder.Services.AddControllers()
            .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        // Manejo global de errores con ProblemDetails
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

        // Swagger / OpenAPI
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API Granja Lechera",
                Version = "v1",
                Description = "Producción de leche, inventario FIFO por lotes, ventas y reportes."
            });

            // Swashbuckle 6.4 no reconoce DateOnly: se documenta como fecha yyyy-MM-dd.
            c.MapType<DateOnly>(() => new OpenApiSchema { Type = "string", Format = "date" });
            c.MapType<DateOnly?>(() => new OpenApiSchema { Type = "string", Format = "date", Nullable = true });
        });

        // configuración AutoMapper
        var mappingConfiguration = new MapperConfiguration(m => m.AddProfile(new MappingProfile()), NullLoggerFactory.Instance);
        IMapper mapper = mappingConfiguration.CreateMapper();
        builder.Services.AddSingleton(mapper);

        // configuración CORS
        builder.Services.AddCors(p => p.AddPolicy("CORS_Policy", policy =>
        {
            policy.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
        }));

        var app = builder.Build();

        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<GranjaDbContext>();

            // Con migraciones generadas se aplican; mientras no existan, se crea el esquema directamente.
            if (dbContext.Database.GetMigrations().Any())
                dbContext.Database.Migrate();
            else
                dbContext.Database.EnsureCreated();
        }

        app.UseExceptionHandler();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseCors("CORS_Policy");

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
