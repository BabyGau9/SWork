using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SWork.Common.Middleware
{
    public class KeyNotFoundExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is KeyNotFoundException)
            {
                var details = new ProblemDetails()
                {
                    Detail = $"An error occurred: {exception.Message}",
                    Instance = "Request",
                    Status = StatusCodes.Status404NotFound,
                    Title = "Key Not Found",
                    Type = "https://httpstatuses.com/404"
                };

                var response = JsonSerializer.Serialize(details);

                httpContext.Response.ContentType = "application/json; charset=utf-8";

                httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

                await httpContext.Response.WriteAsync(response, cancellationToken);

                return true;
            }

            return false;
        }
    }
}
