using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Queries.Product;
using Marten;

namespace Catalog.Application.Commands.Product;

public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductCommandResponse>;

public record DeleteProductCommandResponse(bool Success);

public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, DeleteProductCommandResponse>
{
    private readonly IDocumentSession _documentSession;

    public DeleteProductCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<DeleteProductCommandResponse> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        var product = await _documentSession.LoadAsync<Domain.Aggregates.Product.Product>(command.Id, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(command.Id);
        }

        _documentSession.Delete(product);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return new DeleteProductCommandResponse(true);
    }
}
