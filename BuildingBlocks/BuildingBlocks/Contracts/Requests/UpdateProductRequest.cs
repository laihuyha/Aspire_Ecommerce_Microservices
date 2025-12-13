using System.Collections.Generic;

namespace BuildingBlocks.Contracts.Requests;

public record UpdateProductRequest(
    string Name,
    List<string> Categories,
    string Description,
    string ImageUrl,
    decimal Price);
