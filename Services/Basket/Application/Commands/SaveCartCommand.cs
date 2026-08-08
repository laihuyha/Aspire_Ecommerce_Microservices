using Basket.Domain.Aggregates;
using Basket.Domain.Entities;
using Basket.Domain.Interfaces;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Basket.Application.Commands
{
    public record SaveCartCommandItem(Guid ProductId, string ProductName, string ImageUrl, decimal UnitPrice, int Quantity);

    public record SaveCartCommand(Guid UserId, List<SaveCartCommandItem> Items) : IRequest<SaveCartCommandResponse>;

    public record SaveCartCommandResponse(Guid CartId);

    public class SaveCartCommandHandler : IRequestHandler<SaveCartCommand, SaveCartCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SaveCartCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<SaveCartCommandResponse> Handle(SaveCartCommand request, CancellationToken cancellationToken)
        {
            IEnumerable<ShoppingCartItem> items = request.Items.Select(i =>
                ShoppingCartItem.Create(i.ProductId, i.ProductName, i.ImageUrl, i.UnitPrice, i.Quantity));

            ShoppingCart cart = ShoppingCart.Create(request.UserId, items);

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            await _unitOfWork.ShoppingCarts.AddAsync(cart, cancellationToken);
            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return new SaveCartCommandResponse(cart.Id);
        }
    }
}
