using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SubastaYa.Application.Common.Exceptions;

namespace SubastaYa.API.Middleware;

// Único lugar donde una excepción se convierte en código HTTP. Los controllers no
// llevan try/catch: si aparece un caso nuevo, se agrega acá y vale para toda la API.
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleAsync(context, exception);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail) = exception switch
        {
            NotFoundException ex =>
                (StatusCodes.Status404NotFound, "Recurso no encontrado", ex.Message),

            BusinessRuleException ex =>
                (StatusCodes.Status400BadRequest, "Regla de negocio violada", ex.Message),

            InvalidCredentialsException ex =>
                (StatusCodes.Status401Unauthorized, "Credenciales inválidas", ex.Message),

            UnauthorizedAccessException =>
                (StatusCodes.Status401Unauthorized, "No autenticado", "La petición requiere un token válido."),

            ConflictException ex =>
                (StatusCodes.Status409Conflict, "Conflicto de estado", ex.Message),

            // El corazón del requisito de concurrencia: dos pujas simultáneas sobre la
            // misma subasta y la que pierde la carrera se va con 409, nunca con 500.
            DbUpdateConcurrencyException =>
                (StatusCodes.Status409Conflict, "Conflicto de concurrencia",
                    "Otra operación modificó el recurso mientras procesábamos la tuya. Volvé a intentarlo."),

            _ => (StatusCodes.Status500InternalServerError, "Error interno",
                    "Ocurrió un error inesperado procesando la petición.")
        };

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            _logger.LogError(exception, "Excepción no controlada en {Method} {Path}",
                context.Request.Method, context.Request.Path);
        }
        else
        {
            _logger.LogInformation("{StatusCode} en {Method} {Path}: {Message}",
                statusCode, context.Request.Method, context.Request.Path, exception.Message);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";
        await context.Response.WriteAsJsonAsync(problem);
    }
}
