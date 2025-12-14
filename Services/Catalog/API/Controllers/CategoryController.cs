using System;
using System.ComponentModel;
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
    public class CategoryController : BaseApiController
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("{id:guid}")]
        [EndpointName("Get Category By Id")]
        [EndpointSummary("Get a category by its ID")]
        [Description("Returns a single category by its unique identifier.")]
        [ProducesResponseType(typeof(GetCategoryByIdQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<GetCategoryByIdQueryResponse>> GetCategoryById(
            Guid id,
            CancellationToken cancellationToken)
        {
            GetCategoryByIdQuery query = new(id);
            GetCategoryByIdQueryResponse response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpGet]
        [EndpointName("Get Categories")]
        [EndpointSummary("Get a paginated list of categories")]
        [Description("Returns a paginated list of categories with optional filters.")]
        [ProducesResponseType(typeof(GetCategoriesQueryResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<GetCategoriesQueryResponse>> GetCategories(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] bool? rootCategoriesOnly = null,
            [FromQuery] bool? activeOnly = null,
            CancellationToken cancellationToken = default)
        {
            GetCategoriesQuery query = new(pageNumber, pageSize, rootCategoriesOnly, activeOnly);
            GetCategoriesQueryResponse response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpPost]
        [EndpointName("Create Category")]
        [EndpointSummary("Create a new category")]
        [Description("Accepts a category payload and stores it in the database.")]
        [ProducesResponseType(typeof(CreateCategoryResponse), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<CreateCategoryResponse>> CreateCategory(
            CreateCategoryRequest category,
            CancellationToken cancellationToken)
        {
            CreateCategoryCommand command = category.Adapt<CreateCategoryCommand>();
            CreateCategoryCommandResponse response = await _mediator.Send(command, cancellationToken);
            CreateCategoryResponse result = response.Adapt<CreateCategoryResponse>();
            return CreatedAtAction(nameof(GetCategoryById), new { id = response.CategoryId }, result);
        }

        [HttpPut("{id:guid}")]
        [EndpointName("Update Category")]
        [EndpointSummary("Update an existing category")]
        [Description("Updates a category with the provided payload.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateCategory(
            Guid id,
            UpdateCategoryRequest category,
            CancellationToken cancellationToken)
        {
            UpdateCategoryCommand command = new(
                id,
                category.Name,
                category.Description);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        [EndpointName("Delete Category")]
        [EndpointSummary("Delete a category")]
        [Description("Deletes a category by its unique identifier.")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteCategory(
            Guid id,
            CancellationToken cancellationToken)
        {
            DeleteCategoryCommand command = new(id);
            await _mediator.Send(command, cancellationToken);
            return NoContent();
        }
    }
}
