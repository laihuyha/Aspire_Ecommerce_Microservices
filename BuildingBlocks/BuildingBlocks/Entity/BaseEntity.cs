using System;

namespace BuildingBlocks.Entity;

/// <summary>
/// Standard DDD base entity class for aggregate roots and entities.
/// Includes identity and audit fields.
/// </summary>
public abstract class BaseEntity<TId>
{
    public TId Id { get; set; }
    public DateTime CreatedAt { get; set; }
    public string CreatedBy { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string UpdatedBy { get; set; }
}
