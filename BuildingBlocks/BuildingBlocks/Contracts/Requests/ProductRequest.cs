using System.Collections.Generic;

namespace BuildingBlocks.Contracts.Requests;

public record CreateProductRequest(string Name, List<string> Categories, string Description, string ImageUrl, decimal Price);
