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
    public class EmailNotConfirmedExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<EmailNotConfirmedExceptionHandler> _logger;

        public EmailNotConfirmedExceptionHandler(ILogger<EmailNotConfirmedExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is EmailNotConfirmedException)
            {
                _logger.LogWarning("EmailNotConfirmedExceptionHandler is handling the exception: {Message}", exception.Message);

                var details = new ProblemDetails()
                {
                    Detail = exception.Message,
                    Instance = httpContext.Request.Path,
                    Status = StatusCodes.Status401Unauthorized,
                    Title = "Email Not Confirmed",
                    Type = "https://httpstatuses.com/401"
                };

                var response = JsonSerializer.Serialize(details);
                httpContext.Response.ContentType = "application/json; charset=utf-8";
                httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;
                await httpContext.Response.WriteAsync(response, cancellationToken);

                return true;
            }

            _logger.LogInformation("EmailNotConfirmedExceptionHandler did not handle the exception.");
            return false;
        }
    }
}
