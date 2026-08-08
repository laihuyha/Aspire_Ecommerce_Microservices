using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Basket.Domain.Aggregates;
using Basket.Domain.Interfaces;
using BuildingBlocks.Errors;
using MediatR;

namespace Basket.Application.Commands
{
    public record UpdateCartCommandItem(Guid ProductId, int Quantity);

    public record UpdateCartCommand(Guid UserId, Guid CartId, List<UpdateCartCommandItem> Items) : IRequest<UpdateCartCommandResponse>;

    public record UpdateCartCommandResponse(Guid CartId);

    public class UpdateCartCommandHandler : IRequestHandler<UpdateCartCommand, UpdateCartCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCartCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateCartCommandResponse> Handle(UpdateCartCommand request, CancellationToken cancellationToken)
        {
            ShoppingCart cart = await _unitOfWork.ShoppingCarts.GetTrackedWithItemsByIdAsync(request.CartId, cancellationToken);
            if (cart is null || cart.UserId != request.UserId)
                throw new NotFoundException(nameof(ShoppingCart), request.CartId);

            foreach (var item in request.Items)
            {
                cart.UpdateItem(item.ProductId, item.Quantity);
            }

            // Cart was loaded under change tracking, so removed items are detected as
            // orphans and deleted on save — no explicit UpdateAsync/Update() call needed.
            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new UpdateCartCommandResponse(cart.Id);
        }
    }
}
