using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Interfaces;

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
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateProductCommandResponse> Handle(UpdateProductCommand command,
            CancellationToken cancellationToken)
        {
            Product product = await _unitOfWork.GetByIdAsync<Product>(command.Id, cancellationToken);

            if (product is null)
            {
                throw new ProductNotFoundException(command.Id);
            }

            product.UpdateBasicInfo(command.Name, command.Description, command.ImageUrl);
            product.SetBasePrice(command.BasePrice);

            await _unitOfWork.Repository<Product>().UpdateAsync(product, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UpdateProductCommandResponse(true);
        }
    }
}
