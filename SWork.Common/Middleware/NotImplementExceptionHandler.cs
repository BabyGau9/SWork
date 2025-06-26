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
    public class NotImplementExceptionHandler : IExceptionHandler
    {
        public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is NotImplementedException)
            {
                var details = new ProblemDetails()
                {
                    Detail = $"An error occurred: {exception.Message}",
                    Instance = "API",
                    Status = StatusCodes.Status501NotImplemented,
                    Title = "Not Implement",
                    Type = "https://httpstatuses.com/501"
                };

                var response = JsonSerializer.Serialize(details);

                httpContext.Response.ContentType = "application/json; charset=utf-8";

                httpContext.Response.StatusCode = StatusCodes.Status501NotImplemented;

                await httpContext.Response.WriteAsync(response, cancellationToken);

                return true;
            }

            return false;
        }
    }
}
