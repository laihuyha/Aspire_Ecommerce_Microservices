using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Basket.Application.Commands;
using Basket.Application.Queries;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Basket.Api.Controllers
{
    public class CartController : BaseApiController
    {
        #region Get Cart

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

        #endregion

        #region Interactions

        [HttpPost]
        [EndpointName("Save Cart")]
        [EndpointSummary("Save a cart")]
        [Description("Creates a cart.")]
        [ProducesResponseType(typeof(SaveCartCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<SaveCartCommandResponse>> SaveCart(SaveCartCommand request, CancellationToken cancellationToken)
        {
            SaveCartCommandResponse response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPatch("{id:guid}")]
        [EndpointName("Update Cart Items")]
        [EndpointSummary("Apply item-level changes to a cart")]
        [Description("Applies quantity changes/removals for the specified items; items not listed are left untouched. Every ProductId in the request must already exist in the cart.")]
        [ProducesResponseType(typeof(UpdateCartCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UpdateCartCommandResponse>> UpdateCart(Guid id, UpdateCartCommand request, CancellationToken cancellationToken)
        {
            if (id != request.CartId)
            {
                return BadRequest();
            }

            UpdateCartCommandResponse response = await Mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        #endregion
    }
}