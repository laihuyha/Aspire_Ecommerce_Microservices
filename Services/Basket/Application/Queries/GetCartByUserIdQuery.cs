using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Application.Exceptions;
using Basket.Domain.Interfaces;
using BuildingBlocks.CQRS;

namespace Basket.Application.Queries;

public record GetCartByUserIdQuery(Guid UserId) : IQuery<GetCartByUserIdQueryResponse>;

public record GetCartByUserIdQueryResponse(
    Guid Id,
    Guid UserId,
    decimal Discount,
    decimal Coupon,
    decimal SubTotal,
    decimal Total);

public class GetCartByUserIdQueryHandler : IQueryHandler<GetCartByUserIdQuery, GetCartByUserIdQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCartByUserIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetCartByUserIdQueryResponse> Handle(GetCartByUserIdQuery request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetByUserIdAsync(request.UserId, cancellationToken);
        if (cart is null) throw new CartNotFoundException(request.UserId);

        return new GetCartByUserIdQueryResponse(
            cart.Id,
            cart.UserId,
            cart.Discount,
            cart.Coupon,
            cart.SubTotal,
            cart.Total);
    }
}