using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Commands
{
    public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductCommandResponse>;

    public record DeleteProductCommandResponse(bool Success);

    public class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand, DeleteProductCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteProductCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DeleteProductCommandResponse> Handle(DeleteProductCommand command,
            CancellationToken cancellationToken)
        {
            Product product = await _unitOfWork.GetByIdAsync<Product>(command.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException(command.Id);
            }

            await _unitOfWork.Repository<Product>().DeleteAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new DeleteProductCommandResponse(true);
        }
    }
}
