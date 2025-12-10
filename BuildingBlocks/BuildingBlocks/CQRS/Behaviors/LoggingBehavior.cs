using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;

namespace BuildingBlocks.CQRS.Behaviors;

/// <summary>
/// Pipeline behavior for logging request and response handling.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    private static readonly Action<ILogger, string, TRequest, Exception> _handlingRequest =
        LoggerMessage.Define<string, TRequest>(
            LogLevel.Information,
            new EventId(1, "HandlingRequest"),
            "Handling '{RequestName}' with data: {@Request}");

    private static readonly Action<ILogger, string, TResponse, Exception> _handledRequest =
        LoggerMessage.Define<string, TResponse>(
            LogLevel.Information,
            new EventId(2, "HandledRequest"),
            "Handled '{RequestName}' with response: {@Response}");

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        _handlingRequest(_logger, requestName, request, null);

        var response = await next(cancellationToken);

        _handledRequest(_logger, requestName, response, null);

        return response;
    }
}
