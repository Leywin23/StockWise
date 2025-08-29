using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Net;
using System.Text.Json;

namespace StockWise.Exceptions
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
                var (status, title) = MapToStatus(ex);
                Log(ex, status);

                var apiError = ApiError.From(
                    ex: ShouldExposeDetailToClient(status) ? ex : new Exception("Unexpected error."),
                    status: status,
                    ctx: context,
                    title: title
                );

                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("Response has already started, cannot write error body.");
                    throw;
                }

                context.Response.Clear();
                context.Response.ContentType = "application/json";
                context.Response.StatusCode = status;

                var json = JsonSerializer.Serialize(apiError);
                await context.Response.WriteAsync(json);
            }
        }

        private static (int status, string title) MapToStatus(Exception ex) => ex switch
        {
            // 400
            ArgumentException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
            InvalidOperationException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
            NotSupportedException => ((int)HttpStatusCode.BadRequest, "Bad Request"),
            { } e when e.GetType().FullName == "FluentValidation.ValidationException"
                => ((int)HttpStatusCode.BadRequest, "Validation Failed"),

            // 401 
            UnauthorizedAccessException => ((int)HttpStatusCode.Unauthorized, "Unauthorized"),
            SecurityTokenException => ((int)HttpStatusCode.Unauthorized, "Invalid token"),

            // 403 
            { } e when e.GetType().Name.Contains("NoPermission", StringComparison.OrdinalIgnoreCase)
                => ((int)HttpStatusCode.Forbidden, "Forbidden"),

            // 404 
            KeyNotFoundException => ((int)HttpStatusCode.NotFound, "Not Found"),
            { } e when e.GetType().Name.Contains("NotFound", StringComparison.OrdinalIgnoreCase)
                => ((int)HttpStatusCode.NotFound, "Not Found"),

            // 409 –
            DbUpdateConcurrencyException => ((int)HttpStatusCode.Conflict, "Conflict"),
            DbUpdateException => ((int)HttpStatusCode.Conflict, "Conflict"),

            // 408 / 499 
            TimeoutException => ((int)HttpStatusCode.RequestTimeout, "Request Timeout"),
            TaskCanceledException => ((int)HttpStatusCode.RequestTimeout, "Request Timeout"),
            OperationCanceledException oce when oce.CancellationToken.IsCancellationRequested
                => (499, "Client Closed Request"), 

            // 502 
            HttpRequestException => ((int)HttpStatusCode.BadGateway, "Bad Gateway"),

            // fallback – 500
            _ => ((int)HttpStatusCode.InternalServerError, "Internal Server Error")
        };

        private void Log(Exception ex, int status) {
            if (status >= 500) _logger.LogError(ex, "Unhandled exception");
            else _logger.LogWarning(ex, "Handled domain/application exception");
        }

        private static bool ShouldExposeDetailToClient(int status) => status < 500;
    }
}
