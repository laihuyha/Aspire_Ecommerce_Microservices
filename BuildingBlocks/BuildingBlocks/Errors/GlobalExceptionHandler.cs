using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.Errors
{
    /// <summary>
    ///     Global exception handler for consistent error responses.
    /// </summary>
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
            _logger.LogError(exception, "An error occurred while processing the request");

            ProblemDetails problemDetails = exception switch
            {
                ValidationException ex => CreateValidationProblem(ex),
                NotFoundException ex => CreateNotFoundProblem(ex),
                DomainException ex => CreateDomainProblem(ex),
                _ => CreateInternalServerProblem()
            };

            httpContext.Response.StatusCode = problemDetails.Status ?? (int)HttpStatusCode.InternalServerError;

            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static ProblemDetails CreateValidationProblem(ValidationException exception)
        {
            ProblemDetails problemDetails = new()
            {
                Status = (int)HttpStatusCode.BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Validation Error",
                Detail = "One or more validation errors occurred."
            };

            problemDetails.Extensions["errors"] = exception.Errors;

            return problemDetails;
        }

        private static ProblemDetails CreateNotFoundProblem(NotFoundException exception)
        {
            return new ProblemDetails
            {
                Status = (int)HttpStatusCode.NotFound,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
                Title = "Not Found",
                Detail = $"The {exception.ResourceType} with id '{exception.ResourceId}' was not found."
            };
        }

        private static ProblemDetails CreateDomainProblem(DomainException exception)
        {
            return new ProblemDetails
            {
                Status = (int)HttpStatusCode.BadRequest,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.5.1",
                Title = "Domain Rule Violation",
                Detail = exception.Message
            };
        }

        private static ProblemDetails CreateInternalServerProblem()
        {
            return new ProblemDetails
            {
                Status = (int)HttpStatusCode.InternalServerError,
                Type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later."
            };
        }
    }
}
