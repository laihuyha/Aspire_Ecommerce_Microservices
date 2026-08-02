using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Basket.Application.Exceptions;
using Basket.Domain.Interfaces;
using BuildingBlocks.CQRS;

namespace Basket.Application.Queries;

public record GetCartWithItemsQuery(Guid CartId) : IQuery<GetCartWithItemsQueryResponse>;

public record CartItemResponse(
    Guid Id,
    Guid ProductId,
    string ProductName,
    string ImageUrl,
    decimal UnitPrice,
    int Quantity,
    decimal SubTotal,
    Guid? VariantId,
    string VariantName,
    string SKU);

public record GetCartWithItemsQueryResponse(
    Guid Id,
    Guid UserId,
    decimal Discount,
    decimal Coupon,
    decimal SubTotal,
    decimal Total,
    IReadOnlyCollection<CartItemResponse> Items);

public class GetCartWithItemsQueryHandler : IQueryHandler<GetCartWithItemsQuery, GetCartWithItemsQueryResponse>
{
    private readonly IUnitOfWork _unitOfWork;

    public GetCartWithItemsQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<GetCartWithItemsQueryResponse> Handle(GetCartWithItemsQuery request, CancellationToken cancellationToken)
    {
        var cart = await _unitOfWork.ShoppingCarts.GetWithItemsByIdAsync(request.CartId, cancellationToken);
        if (cart is null) throw new CartNotFoundException(request.CartId);

        var items = cart.Items.Select(item => new CartItemResponse(
            item.Id,
            item.ProductId,
            item.ProductName,
            item.ImageUrl,
            item.UnitPrice,
            item.Quantity,
            item.SubTotal,
            item.VariantId,
            item.VariantName,
            item.SKU)).ToList();

        return new GetCartWithItemsQueryResponse(
            cart.Id,
            cart.UserId,
            cart.Discount,
            cart.Coupon,
            cart.SubTotal,
            cart.Total,
            items);
    }
}