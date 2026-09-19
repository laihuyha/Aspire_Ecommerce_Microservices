using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using Basket.Domain.Interfaces;
using BuildingBlocks.Errors;
using MediatR;

namespace Basket.Application.Commands
{

    public record DeleteCartCommand(Guid UserId) : IRequest<DeleteCartCommandResponse>;

    public record DeleteCartCommandResponse(bool Success);

    public class DeleteCartCommandHandler : IRequestHandler<DeleteCartCommand, DeleteCartCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCartCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DeleteCartCommandResponse> Handle(DeleteCartCommand request, CancellationToken cancellationToken)
        {
            ShoppingCart cart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(request.UserId, cancellationToken);

            if (cart == null)
            {
                throw new NotFoundException($"Shopping cart for user {request.UserId} not found.");
            }

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.ShoppingCarts.DeleteAsync(cart, cancellationToken);
            var result = await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new DeleteCartCommandResponse(result);
        }
    }
}