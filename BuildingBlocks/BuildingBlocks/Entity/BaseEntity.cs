using System;
using System.Collections.Generic;

namespace BuildingBlocks.Entity
{
    /// <summary>
    ///     Standard DDD base entity class for aggregate roots and entities.
    ///     Includes identity, audit fields, and domain events support.
    /// </summary>
    public abstract class BaseEntity<TId>
    {
        private readonly List<DomainEvent> _domainEvents = new();

        public TId Id { get; set; } = default!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;

        /// <summary>
        ///     Gets the domain events raised by this entity.
        /// </summary>
        public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents.AsReadOnly();

        /// <summary>
        ///     Adds a domain event to this entity.
        /// </summary>
        /// <param name="domainEvent">The domain event to add.</param>
        public void AddDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Add(domainEvent);
        }

        /// <summary>
        ///     Removes a domain event from this entity.
        /// </summary>
        /// <param name="domainEvent">The domain event to remove.</param>
        public void RemoveDomainEvent(DomainEvent domainEvent)
        {
            _domainEvents.Remove(domainEvent);
        }

        /// <summary>
        ///     Clears all domain events from this entity.
        /// </summary>
        public void ClearDomainEvents()
        {
            _domainEvents.Clear();
        }
    }
}
