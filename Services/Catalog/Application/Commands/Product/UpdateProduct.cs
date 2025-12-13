using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Queries.Product;
using Marten;

namespace Catalog.Application.Commands.Product;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    List<string> Categories,
    string Description,
    string ImageUrl,
    decimal Price) : ICommand<UpdateProductCommandResponse>;

public record UpdateProductCommandResponse(bool Success);

public class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, UpdateProductCommandResponse>
{
    private readonly IDocumentSession _documentSession;

    public UpdateProductCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<UpdateProductCommandResponse> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _documentSession.LoadAsync<Domain.Aggregates.Product.Product>(command.Id, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(command.Id);
        }

        product.UpdateDetails(command.Name, command.Description, command.ImageUrl);
        product.UpdatePrice(command.Price);

        // Clear and re-add categories
        foreach (var category in command.Categories ?? new List<string>())
        {
            product.AddCategory(category);
        }

        _documentSession.Store(product);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return new UpdateProductCommandResponse(true);
    }
}
