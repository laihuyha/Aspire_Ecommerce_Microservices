using System;
using MediatR;

namespace BuildingBlocks.Entity
{
    /// <summary>
    ///     Base class for domain events.
    ///     Provides common properties like occurence time.
    /// </summary>
    public abstract class DomainEvent : INotification
    {
        public DateTime OccurredOn { get; } = DateTime.UtcNow;
    }
}
