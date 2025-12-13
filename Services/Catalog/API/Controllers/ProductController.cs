using System.ComponentModel;
using System.Threading.Tasks;
using BuildingBlocks.Contracts.Requests;
using BuildingBlocks.Contracts.Responses;
using Catalog.Application.Commands;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Controllers;

// [Authorize]
public class ProductController : BaseApiController
{
    private readonly IMediator _mediator;

    public ProductController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("create")]
    [EndpointName("Create Product")]
    [EndpointSummary("Create a new product")]
    [Description("Accepts a product payload and stores it in the database.")]
    [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateProductResponse>> CreateProduct(CreateProductRequest product)
    {
        var command = product.Adapt<CreateProductCommand>();
        var response = await _mediator.Send(command);
        var result = response.Adapt<CreateProductResponse>();
        return CreatedAtAction(nameof(CreateProduct), new { id = response.ProductId }, result);
    }
}
