using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Catalog.Api.Requests;
using Catalog.Api.Responses;
using Catalog.Application.Commands;
using Catalog.Application.Queries;
using Mapster;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace Catalog.Api.Controllers
{
    public class ProductController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ProductController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:guid}")]
        [EndpointName("Get Product By Id")]
        [EndpointSummary("Get a product by its ID")]
        [Description("Returns a single product by its unique identifier.")]
        [ProducesResponseType(typeof(GetProductByIdQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetProductByIdQueryResponse>> GetProductById(
            Guid id,
            CancellationToken cancellationToken)
        {
            GetProductByIdQuery query = new(id);
            GetProductByIdQueryResponse response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpGet]
        [EndpointName("Get Products")]
        [EndpointSummary("Get a paginated list of products")]
        [Description("Returns a paginated list of products with optional category filter.")]
        [ProducesResponseType(typeof(GetProductsQueryResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GetProductsQueryResponse>> GetProducts(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string category = null,
            CancellationToken cancellationToken = default)
        {
            GetProductsQuery query = new(pageNumber, pageSize, category);
            GetProductsQueryResponse response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [EndpointName("Create Product")]
        [EndpointSummary("Create a new product")]
        [Description("Accepts a product payload and stores it in the database.")]
        [ProducesResponseType(typeof(CreateProductResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateProductResponse>> CreateProduct(
            CreateProductRequest product,
            CancellationToken cancellationToken)
        {
            var categories = product.Categories?.Select(id =>
                    new Catalog.Application.Commands.CategoryInfo(id, $"Category-{id.ToString()[..8]}"))
                .ToList();

            CreateProductCommand command = new(
                product.Name,
                product.Description,
                product.ImageUrl,
                product.BasePrice,
                categories);

            CreateProductCommandResponse response = await _mediator.Send(command, cancellationToken);
            CreateProductResponse result = response.Adapt<CreateProductResponse>();
            return CreatedAtAction(nameof(GetProductById), new { id = response.ProductId }, result);
        }

        [HttpPut("{id:guid}")]
        [EndpointName("Update Product")]
        [EndpointSummary("Update an existing product")]
        [Description("Updates a product with the provided payload.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProduct(
            Guid id,
            UpdateProductRequest product,
            CancellationToken cancellationToken)
        {
            UpdateProductCommand command = new(
                id,
                product.Name,
                product.Description,
                product.ImageUrl,
                product.BasePrice);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [EndpointName("Delete Product")]
        [EndpointSummary("Delete a product")]
        [Description("Deletes a product by its unique identifier.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteProduct(
            Guid id,
            CancellationToken cancellationToken)
        {
            DeleteProductCommand command = new(id);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
