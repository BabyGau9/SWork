using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace SWork.Common.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Random Exception has spawned");

            if (exception is not UnauthorizedAccessException && exception is not KeyNotFoundException && exception is not NotImplementedException && exception is not BadHttpRequestException)
            {
                var details = new ProblemDetails()
                {
                    Detail = $"An error occurred: {exception.Message}",
                    Instance = "API",
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "Internal Server Error",
                    Type = "https://httpstatuses.com/500"
                };

                var response = JsonSerializer.Serialize(details);

                httpContext.Response.ContentType = "application/json; charset=utf-8";
                httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

                await httpContext.Response.WriteAsync(response, cancellationToken);

                return true;
            }
            return false;
        }
    }
}
