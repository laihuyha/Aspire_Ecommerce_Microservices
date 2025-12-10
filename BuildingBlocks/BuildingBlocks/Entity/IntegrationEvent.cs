using System;

namespace BuildingBlocks.Entity;

/// <summary>
/// Base class for integration events sent between services.
/// </summary>
public abstract class IntegrationEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
}
