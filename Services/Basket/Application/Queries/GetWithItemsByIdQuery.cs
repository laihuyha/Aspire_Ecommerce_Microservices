using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Application.Exceptions;
using Basket.Domain.Aggregates;
using Basket.Domain.Interfaces;
using BuildingBlocks.CQRS;
using Domain.Interfaces;

namespace Application.Queries;

public record GetCartWithItemsQuery(Guid CartId) : IQuery<ShoppingCart>;

public class GetCartWithItemsQueryHandler : IQueryHandler<GetCartWithItemsQuery, ShoppingCart>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCartWithItemsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ShoppingCart> Handle(GetCartWithItemsQuery request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetWithItemsByIdAsync(request.CartId, cancellationToken);
        if (cart is null) throw new CartNotFoundException(request.CartId);
        return cart;
    }
}