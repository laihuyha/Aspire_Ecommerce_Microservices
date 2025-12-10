using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.CQRS.Behaviors;

/// <summary>
/// Pipeline behavior for global error handling and logging.
/// </summary>
public class ErrorHandlingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<ErrorHandlingBehavior<TRequest, TResponse>> _logger;

    private static readonly Action<ILogger, string, string, object, Exception> _requestFailed = LoggerMessage.Define<string, string, object>(
        LogLevel.Error,
        new EventId(0, "RequestFailed"),
        "Request '{RequestName}' failed with error: {ErrorMessage}. Request data: {@Request}");

    public ErrorHandlingBehavior(ILogger<ErrorHandlingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        try
        {
            return await next(cancellationToken);
        }
        catch (Exception exception)
        {
            var requestName = typeof(TRequest).Name;
            _requestFailed(_logger, requestName, exception.Message, request, exception);

            // Re-throw or handle with custom exceptions if needed
            throw;
        }
    }
}
