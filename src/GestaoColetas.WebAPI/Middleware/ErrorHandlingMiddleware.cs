using System.Net;
using System.Text.Json;

namespace GestaoColetas.WebAPI.Middleware;

/// <summary>
/// Captura exceções em qualquer ponto da requisição e devolve uma resposta HTTP
/// limpa (com o status certo) em vez de um erro feio. Também registra logs.
/// </summary>
public class ErrorHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlingMiddleware> _logger;

    public ErrorHandlingMiddleware(RequestDelegate next, ILogger<ErrorHandlingMiddleware> logger)
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
            var (status, mensagem) = ex switch
            {
                KeyNotFoundException => (HttpStatusCode.NotFound, ex.Message),
                InvalidOperationException => (HttpStatusCode.BadRequest, ex.Message),
                ArgumentException => (HttpStatusCode.BadRequest, ex.Message),
                _ => (HttpStatusCode.InternalServerError, "Ocorreu um erro inesperado.")
            };

            if (status == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Erro não tratado");
            else
                _logger.LogWarning("Requisição rejeitada: {Mensagem}", ex.Message);

            context.Response.StatusCode = (int)status;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsync(JsonSerializer.Serialize(new { erro = mensagem }));
        }
    }
}
