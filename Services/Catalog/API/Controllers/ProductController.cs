using System.ComponentModel;
using System.Threading.Tasks;
using API.DTO;
using API.Responses;
using Application.Commands;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Controllers;

// [Authorize]
public class ProductController : BaseApiController
{
    private readonly IMediator _mediatR;

    public ProductController(IMediator mediatR)
    {
        _mediatR = mediatR;
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
        var response = await _mediatR.Send(command);
        var result = response.Adapt<CreateProductResponse>();
        return CreatedAtAction(nameof(CreateProduct), new { id = response.ProductId }, result);
    }
}
