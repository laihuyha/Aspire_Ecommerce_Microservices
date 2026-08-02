using System;
using System.Threading;
using System.Threading.Tasks;
using Basket.Application.Exceptions;
using Basket.Domain.Interfaces;
using BuildingBlocks.CQRS;

namespace Basket.Application.Queries;

public record GetCartByIdQuery(Guid CartId) : IQuery<GetCartByIdQueryResponse>;

public record GetCartByIdQueryResponse(
    Guid Id,
    Guid UserId,
    decimal Discount,
    decimal Coupon,
    decimal SubTotal,
    decimal Total);

public class GetCartByIdQueryHandler : IQueryHandler<GetCartByIdQuery, GetCartByIdQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCartByIdQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetCartByIdQueryResponse> Handle(GetCartByIdQuery request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetByIdAsync(request.CartId, cancellationToken);
        if (cart is null) throw new CartNotFoundException(request.CartId);

        return new GetCartByIdQueryResponse(
            cart.Id,
            cart.UserId,
            cart.Discount,
            cart.Coupon,
            cart.SubTotal,
            cart.Total);
    }
}