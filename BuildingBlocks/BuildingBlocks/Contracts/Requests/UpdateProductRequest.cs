namespace BuildingBlocks.Contracts.Requests;

public record UpdateProductRequest(
    string Name,
    string Description,
    string ImageUrl,
    decimal? BasePrice);
