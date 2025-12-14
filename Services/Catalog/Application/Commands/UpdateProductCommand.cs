using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Product;
using Marten;

namespace Catalog.Application.Commands
{
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

        public async Task<UpdateProductCommandResponse> Handle(UpdateProductCommand command,
            CancellationToken cancellationToken)
        {
            Product product = await _documentSession.LoadAsync<Product>(command.Id, cancellationToken);

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
}
