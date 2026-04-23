using Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;

namespace Api.Middleware;

public static class ExceptionHandlerExtensions
{
    public static IApplicationBuilder UseCustomExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(exceptionHandlerApp =>
        {
            exceptionHandlerApp.Run(async context =>
            {
                var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = exceptionFeature?.Error;

                var pds = context.RequestServices.GetService<IProblemDetailsService>();
                var problemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails();

                switch (exception)
                {
                    case ValidationException validationEx:
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        problemDetails.Status = StatusCodes.Status400BadRequest;
                        problemDetails.Title = "Validation Failure";
                        problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
                        problemDetails.Detail = validationEx.Message;
                        problemDetails.Extensions["errors"] = validationEx.Errors;
                        break;

                    case NotFoundException notFoundEx:
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        problemDetails.Status = StatusCodes.Status404NotFound;
                        problemDetails.Title = "Not Found";
                        problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4";
                        problemDetails.Detail = notFoundEx.Message;
                        break;

                    default:
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        problemDetails.Status = StatusCodes.Status500InternalServerError;
                        problemDetails.Title = "An error occurred while processing your request.";
                        problemDetails.Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
                        break;
                }

                if (pds != null)
                {
                    await pds.TryWriteAsync(new ProblemDetailsContext
                    {
                        HttpContext = context,
                        ProblemDetails = problemDetails
                    });
                }
            });
        });

        return app;
    }
}
