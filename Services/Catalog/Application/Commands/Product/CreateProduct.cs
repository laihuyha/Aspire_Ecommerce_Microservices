using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Domain;
using Marten;

namespace Application.Commands;

public record CreateProductCommand(
    string Name,
    List<string> Categories,
    string Description,
    string ImageUrl,
    decimal Price) : ICommand<CreateProductCommandResponse>;

public record CreateProductCommandResponse(Guid ProductId);

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductCommandResponse>
{
    private readonly IDocumentSession _document;

    public CreateProductCommandHandler(IDocumentSession document)
    {
        _document = document;
    }

    public async Task<CreateProductCommandResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Product.Create(
            command.Name,
            command.Price,
            command.Description,
            command.ImageUrl
        );

        foreach (var category in command.Categories ?? [])
        {
            product.AddCategory(category);
        }

        _document.Store(product);
        await _document.SaveChangesAsync(cancellationToken);

        return new CreateProductCommandResponse(product.Id);
    }
}
