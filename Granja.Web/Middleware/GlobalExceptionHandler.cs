using Granja.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Granja.Web.Middleware;

/// <summary>
/// Manejo centralizado de errores: convierte las excepciones de dominio en respuestas ProblemDetails
/// con el código HTTP correcto (400, 404, 409) y deja 500 para lo inesperado.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IProblemDetailsService _problemDetails;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IProblemDetailsService problemDetails, ILogger<GlobalExceptionHandler> logger)
    {
        _problemDetails = problemDetails;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        var (status, title) = exception switch
        {
            ValidacionException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            EntidadNoEncontradaException => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            InventarioInsuficienteException => (StatusCodes.Status409Conflict, "Inventario insuficiente"),
            LecheInsuficienteException => (StatusCodes.Status409Conflict, "Leche insuficiente"),
            ProduccionYaRegistradaException => (StatusCodes.Status409Conflict, "Producción ya registrada"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor")
        };

        string detail;
        if (status == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Error no controlado procesando {Method} {Path}", httpContext.Request.Method, httpContext.Request.Path);
            detail = "Ocurrió un error inesperado.";
        }
        else
        {
            detail = exception.Message;
        }

        httpContext.Response.StatusCode = status;

        return await _problemDetails.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            Exception = exception,
            ProblemDetails = new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            }
        });
    }
}
