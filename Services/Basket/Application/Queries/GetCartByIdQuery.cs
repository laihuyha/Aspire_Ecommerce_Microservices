using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Application.Exceptions;
using Basket.Domain.Aggregates;
using BuildingBlocks.CQRS;
using Domain.Interfaces;

namespace Application.Queries;
public record GetCartByIdQuery(Guid CartId) : IQuery<ShoppingCart>;

public class GetCartByIdQueryHandler : IQueryHandler<GetCartByIdQuery, ShoppingCart>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCartByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ShoppingCart> Handle(GetCartByIdQuery request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetByIdAsync(request.CartId, cancellationToken);
        if (cart is null) throw new CartNotFoundException(request.CartId);
        return cart;
    }
}