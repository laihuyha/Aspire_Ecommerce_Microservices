using System;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Domain.Aggregates.Product.Events;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Catalog.Application.Handlers
{
    internal static class Log
    {
        internal static readonly Action<ILogger, Guid, DateTime, Exception> ProductCreated =
            LoggerMessage.Define<Guid, DateTime>(
                LogLevel.Information,
                new EventId(1, "ProductCreated"),
                "Product created with ID: {ProductId} at {OccurredOn}");
    }

    /// <summary>
    ///     Handler for ProductCreatedDomainEvent.
    ///     Performs side effects like logging, notifications, etc.
    /// </summary>
    public class ProductCreatedNotificationHandler : INotificationHandler<ProductCreatedDomainEvent>
    {
        private readonly ILogger<ProductCreatedNotificationHandler> _logger;

        public ProductCreatedNotificationHandler(ILogger<ProductCreatedNotificationHandler> logger)
        {
            _logger = logger;
        }

        public Task Handle(ProductCreatedDomainEvent notification, CancellationToken cancellationToken)
        {
            Log.ProductCreated(_logger, notification.ProductId, notification.OccurredOn, null);

            // TODO: Send notification, update cache, trigger integrations, etc.
            // e.g., await _emailService.SendProductCreatedNotification(notification.ProductId);

            return Task.CompletedTask;
        }
    }
}
