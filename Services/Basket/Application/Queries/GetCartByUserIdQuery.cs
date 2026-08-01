using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Application.Exceptions;
using Basket.Domain.Aggregates;
using BuildingBlocks.CQRS;
using Domain.Interfaces;

namespace Application.Queries;

public record GetCartByUserIdQuery(Guid UserId) : IQuery<ShoppingCart>;

public class GetCartByUserIdQueryHandler : IQueryHandler<GetCartByUserIdQuery, ShoppingCart>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCartByUserIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ShoppingCart> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null) throw new CartNotFoundException(request.UserId);
        return cart;
    }
}