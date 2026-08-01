using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Application.Queries;
using Basket.Domain.Aggregates;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Services.Basket.API.Controllers
{
    public class CartController : BaseApiController
    {
        [HttpGet("{id:guid}")]
        [EndpointName("Get Cart By Id")]
        [EndpointSummary("Get a cart by its ID")]
        [Description("Returns a single cart by its unique identifier.")]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShoppingCart>> GetCartById(Guid id, CancellationToken cancellationToken)
        {
            GetCartByIdQuery query = new(id);
            ShoppingCart response = await Mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpGet("user/{userId:guid}")]
        [EndpointName("Get Cart By User Id")]
        [EndpointSummary("Get a cart by its user ID")]
        [Description("Returns a single cart by its user's unique identifier.")]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShoppingCart>> GetCartByUserId(Guid userId, CancellationToken cancellationToken)
        {
            GetCartByUserIdQuery query = new(userId);
            ShoppingCart response = await Mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpGet("with-items/{cartId:guid}")]
        [EndpointName("Get Cart With Items")]
        [EndpointSummary("Get a cart with its items")]
        [Description("Returns a single cart with its items by its unique identifier.")]
        [ProducesResponseType(typeof(ShoppingCart), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ShoppingCart>> GetCartWithItems(Guid cartId, CancellationToken cancellationToken)
        {
            GetCartWithItemsQuery query = new(cartId);
            ShoppingCart response = await Mediator.Send(query, cancellationToken);
            return Ok(response);
        }
    }
}