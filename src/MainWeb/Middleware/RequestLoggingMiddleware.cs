using System.Diagnostics;

namespace MainWeb.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var stopwatch = Stopwatch.StartNew();
            var request = context.Request;

            _logger.LogInformation("HTTP Request: {Method} {Path}", request.Method, request.Path);

            try
            {
                await _next(context);
                stopwatch.Stop();

                _logger.LogInformation(
                    "HTTP Response: {Method} {Path} responded {StatusCode} in {ElapsedMilliseconds}ms",
                    request.Method,
                    request.Path,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Unhandled Exception during HTTP {Method} {Path} after {ElapsedMilliseconds}ms",
                    request.Method,
                    request.Path,
                    stopwatch.ElapsedMilliseconds);

                throw;
            }
        }
    }
}
