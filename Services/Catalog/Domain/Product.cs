using System.Collections.Generic;
using BuildingBlocks.Enity;

namespace Domain;

public class Product : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public List<string> Category { get; set; } = new();
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal Price { get; set; }
}
