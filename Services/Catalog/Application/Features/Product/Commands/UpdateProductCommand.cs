using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Features.Product.Exceptions;
using Marten;

namespace Catalog.Application.Features.Product.Commands;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    string ImageUrl,
    decimal? BasePrice) : ICommand<UpdateProductCommandResponse>;

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

        product.UpdateBasicInfo(command.Name, command.Description, command.ImageUrl);
        product.SetBasePrice(command.BasePrice);

        _documentSession.Store(product);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return new UpdateProductCommandResponse(true);
    }
}
