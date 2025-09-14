using System.Collections.Generic;

namespace API.DTO;

public record CreateProductRequest(string Name, List<string> Category, string Description, string ImageUrl, decimal Price);