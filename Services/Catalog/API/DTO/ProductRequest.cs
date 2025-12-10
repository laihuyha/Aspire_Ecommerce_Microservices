using System.Collections.Generic;

namespace API.DTO;

public record CreateProductRequest(string Name, List<string> Categories, string Description, string ImageUrl, decimal Price);