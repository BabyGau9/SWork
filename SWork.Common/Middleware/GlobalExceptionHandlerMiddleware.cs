
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace SWork.Common.Middleware
{
    public class GlobalExceptionHandlerMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var problem = ex switch
                {
                    BadRequestException => new ProblemDetails
                    {
                        Title = "Bad Request",
                        Status = StatusCodes.Status400BadRequest,
                        Detail = ex.Message,
                        Type = "https://httpstatuses.com/400"
                    },
                    NotFoundException => new ProblemDetails
                    {
                        Title = "Not Found",
                        Status = StatusCodes.Status404NotFound,
                        Detail = ex.Message,
                        Type = "https://httpstatuses.com/404"
                    },
                    ForbiddenException => new ProblemDetails
                    {
                        Title = "Forbidden",
                        Status = StatusCodes.Status403Forbidden,
                        Detail = ex.Message,
                        Type = "https://httpstatuses.com/403"
                    },
                    _ => new ProblemDetails
                    {
                        Title = "Internal Server Error",
                        Status = StatusCodes.Status500InternalServerError,
                        Detail = ex.Message,
                        Type = "https://httpstatuses.com/500"
                    }
                };

                context.Response.ContentType = "application/json";
                context.Response.StatusCode = problem.Status ?? 500;

                await context.Response.WriteAsync(JsonSerializer.Serialize(problem));
            }
        }
    }
    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }   
}
