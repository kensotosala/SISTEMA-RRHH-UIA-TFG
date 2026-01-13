using BusinessLogicLayer.Shared;
using System.Text.Json;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware>? _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware>? logger = null)
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
        catch (InvalidOperationException ex)
        {
            // Este es el error que se lanza cuando tiene subordinados
            _logger?.LogWarning(ex, "Operación no permitida: {Message}", ex.Message);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            await WriteError(context, "OPERATION_NOT_ALLOWED", ex.Message);
        }
        catch (BusinessException ex)
        {
            _logger?.LogWarning(ex, "Error de negocio: {Code} - {Message}", ex.Code, ex.Message);

            context.Response.StatusCode = StatusCodes.Status409Conflict;
            context.Response.ContentType = "application/json";

            var response = new
            {
                error = ex.Code,
                message = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (ArgumentException ex)
        {
            _logger?.LogWarning(ex, "Argumento inválido: {Message}", ex.Message);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            await WriteError(context, "ARGUMENT_ERROR", ex.Message);
        }
        catch (KeyNotFoundException ex)
        {
            _logger?.LogWarning(ex, "Recurso no encontrado: {Message}", ex.Message);

            context.Response.StatusCode = StatusCodes.Status404NotFound;
            await WriteError(context, "NOT_FOUND", ex.Message);
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error inesperado: {Message}", ex.Message);

            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            await WriteError(context, "INTERNAL_ERROR", "Ocurrió un error inesperado.");
        }
    }

    private static async Task WriteError(
        HttpContext context,
        string code,
        string message)
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = code,
            message
        };

        var options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, options));
    }
}