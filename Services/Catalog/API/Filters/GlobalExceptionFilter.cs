using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Logging;

namespace Catalog.Api.Filters
{
    /// <summary>
    ///     Global exception filter for handling unexpected errors globally.
    ///     Returns standardized error responses.
    /// </summary>
    public class GlobalExceptionFilter : IExceptionFilter
    {
        private static readonly Action<ILogger, string, Exception> _unhandledExceptionLogged =
            LoggerMessage.Define<string>(
                LogLevel.Error,
                new EventId(1, "UnhandledException"),
                "Unhandled exception occurred while processing {RequestPath}");

        private readonly ILogger<GlobalExceptionFilter> _logger;

        public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger)
        {
            _logger = logger;
        }

        public void OnException(ExceptionContext context)
        {
            _unhandledExceptionLogged(_logger, context.HttpContext.Request.Path.Value ?? "unknown", context.Exception);

            var response = new
            {
                Type = "https://tools.ietf.org/html/rfc9110",
                Title = "An error occurred while processing your request.",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "Internal server error.",
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new ObjectResult(response) { StatusCode = StatusCodes.Status500InternalServerError };

            context.ExceptionHandled = true;
        }
    }
}
