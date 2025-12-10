using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Catalog.Api.Filters;

/// <summary>
/// Global exception filter for handling unexpected errors globally.
/// Returns standardized error responses.
/// </summary>
public class GlobalExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var response = new
        {
            Type = "https://tools.ietf.org/html/rfc9110",
            Title = "An error occurred while processing your request.",
            Status = StatusCodes.Status500InternalServerError,
            Detail = "Internal server error.",
            Instance = context.HttpContext.Request.Path
        };

        context.Result = new ObjectResult(response)
        {
            StatusCode = StatusCodes.Status500InternalServerError
        };

        context.ExceptionHandled = true;

        // Log the exception if logging is configured
    }
}
