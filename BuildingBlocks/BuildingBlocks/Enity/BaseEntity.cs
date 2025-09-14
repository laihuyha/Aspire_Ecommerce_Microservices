using System;

namespace BuildingBlocks.Enity;

public class BaseEntity
{
    public Guid Id { get; set; }
    public Guid? CreatedBy { get; set; }
    public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;
    public Guid? LastModifiedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}
