using FluentValidation;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace MainWeb.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var (statusCode, problemDetails) = exception switch
            {
                ValidationException validationException => (
                    StatusCodes.Status400BadRequest,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status400BadRequest,
                        Title = "Validation Failed",
                        Detail = "One or more validation errors occurred.",
                        Extensions =
                        {
                        ["errors"] = validationException.Errors
                            .GroupBy(e => e.PropertyName)
                            .ToDictionary(
                                g => g.Key,
                                g => g.Select(e => e.ErrorMessage).ToArray())
                        }
                    }),

                KeyNotFoundException notFoundException => (
                    StatusCodes.Status404NotFound,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status404NotFound,
                        Title = "Resource Not Found",
                        Detail = notFoundException.Message
                    }),

                InvalidOperationException invalidOpException => (
                    StatusCodes.Status409Conflict,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status409Conflict,
                        Title = "Business Rule Violation",
                        Detail = invalidOpException.Message
                    }),

                _ => (
                    StatusCodes.Status500InternalServerError,
                    new ProblemDetails
                    {
                        Status = StatusCodes.Status500InternalServerError,
                        Title = "Server Error",
                        Detail = "An unexpected error occurred."
                    })
            };

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled Exception [{StatusCode}]: {Message}",
                    statusCode,
                    exception.Message);
            }
            else if (statusCode >= StatusCodes.Status400BadRequest)
            {
                _logger.LogWarning(
                    "Handled Business Exception [{StatusCode}]: {Message}",
                    statusCode,
                    exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }
    }
}
