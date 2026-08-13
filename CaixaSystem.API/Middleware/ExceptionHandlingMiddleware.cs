namespace CaixaSystem.API.Middleware;

using CaixaSystem.API.Models;
using CaixaSystem.Domain.Exceptions;
using System.Text.Json;

/// <summary>
/// Middleware para tratamento global de exceções
/// </summary>
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro não tratado: {ExceptionMessage}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            InvalidTransactionException ex => new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = ex.Message,
                error = "InvalidTransaction"
            },
            DailyBalanceConsolidationException ex => new
            {
                statusCode = StatusCodes.Status500InternalServerError,
                message = ex.Message,
                error = "ConsolidationError"
            },
            NotFoundException ex => new
            {
                statusCode = StatusCodes.Status404NotFound,
                message = ex.Message,
                error = "NotFound"
            },
            ArgumentException ex => new
            {
                statusCode = StatusCodes.Status400BadRequest,
                message = ex.Message,
                error = "InvalidArgument"
            },
            _ => new
            {
                statusCode = StatusCodes.Status500InternalServerError,
                message = "Um erro interno ocorreu. Tente novamente mais tarde.",
                error = "InternalServerError"
            }
        };

        context.Response.StatusCode = response.statusCode;
        return context.Response.WriteAsJsonAsync(response);
    }
}

/// <summary>
/// Extensões para registrar o middleware de tratamento de exceções
/// </summary>
public static class ExceptionHandlingExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
