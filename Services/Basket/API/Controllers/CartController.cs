using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Basket.Application.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Basket.Api.Controllers
{
    public class CartController : BaseApiController
    {
        [HttpGet("{id:guid}")]
        [EndpointName("Get Cart By Id")]
        [EndpointSummary("Get a cart by its ID")]
        [Description("Returns a single cart by its unique identifier.")]
        [ProducesResponseType(typeof(GetCartByIdQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCartByIdQueryResponse>> GetCartById(Guid id, CancellationToken cancellationToken)
        {
            GetCartByIdQuery query = new(id);
            GetCartByIdQueryResponse response = await Mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpGet("user/{userId:guid}")]
        [EndpointName("Get Cart By User Id")]
        [EndpointSummary("Get a cart by its user ID")]
        [Description("Returns a single cart by its user's unique identifier.")]
        [ProducesResponseType(typeof(GetCartByUserIdQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCartByUserIdQueryResponse>> GetCartByUserId(Guid userId, CancellationToken cancellationToken)
        {
            GetCartByUserIdQuery query = new(userId);
            GetCartByUserIdQueryResponse response = await Mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpGet("with-items/{cartId:guid}")]
        [EndpointName("Get Cart With Items")]
        [EndpointSummary("Get a cart with its items")]
        [Description("Returns a single cart with its items by its unique identifier.")]
        [ProducesResponseType(typeof(GetCartWithItemsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCartWithItemsQueryResponse>> GetCartWithItems(Guid cartId, CancellationToken cancellationToken)
        {
            GetCartWithItemsQuery query = new(cartId);
            GetCartWithItemsQueryResponse response = await Mediator.Send(query, cancellationToken);
            return Ok(response);
        }
    }
}