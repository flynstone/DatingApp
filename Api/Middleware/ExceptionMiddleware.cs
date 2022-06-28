using Api.Errors;
using System.Net;
using System.Text.Json;

namespace Api.Middleware
{
    public class ExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger _logger;
        private readonly IHostEnvironment _env;
        public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger, IHostEnvironment env)
        {
            _env = env;
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // Await context.
                await _next(context);
            }
            catch (Exception ex)
            {
                // Log error in terminal.
                _logger.LogError(ex, ex.Message);
                // Type of response.
                context.Response.ContentType = "application/json";
                // Set response status code.
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

                // Check if app is in development or production and proceed accordingly.
                var response = _env.IsDevelopment()
                    ? new ApiException(context.Response.StatusCode, ex.Message, ex.StackTrace?.ToString())
                    : new ApiException(context.Response.StatusCode, "Internal Server Error");

                // Make sure the reponse has json case policies.
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

                // Serialize options.
                var json = JsonSerializer.Serialize(response, options);

                await context.Response.WriteAsync(json);
            }
        }
    }
}
