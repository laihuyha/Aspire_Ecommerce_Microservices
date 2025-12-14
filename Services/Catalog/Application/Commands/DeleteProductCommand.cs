using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Product;
using Marten;

namespace Catalog.Application.Commands
{
    public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductCommandResponse>;

    public record DeleteProductCommandResponse(bool Success);

    public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, DeleteProductCommandResponse>
    {
        private readonly IDocumentSession _documentSession;

        public DeleteProductCommandHandler(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        public async Task<DeleteProductCommandResponse> Handle(DeleteProductCommand command,
            CancellationToken cancellationToken)
        {
            Product product = await _documentSession.LoadAsync<Product>(command.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException(command.Id);
            }

            _documentSession.Delete(product);
            await _documentSession.SaveChangesAsync(cancellationToken);

            return new DeleteProductCommandResponse(true);
        }
    }
}
