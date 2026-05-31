using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.Common;
using BuildingBlocks.CQRS;
using Catalog.Domain.Aggregates.Category;
using Catalog.Domain.Interfaces;
using Catalog.Domain.Specifications;

namespace Catalog.Application.Queries
{
    public record GetCategoriesQuery(
        int PageNumber = 1,
        int PageSize = 10,
        bool? RootCategoriesOnly = null,
        bool? ActiveOnly = null) : IQuery<GetCategoriesQueryResponse>;

    public record GetCategoriesQueryResponse(
        IReadOnlyList<CategoryDto> Categories,
        int TotalCount,
        int PageNumber,
        int PageSize);

    public record CategoryDto(
        Guid Id,
        string Name,
        string Description,
        Guid? ParentCategoryId,
        bool IsActive,
        bool IsRootCategory);

    public class GetCategoriesQueryHandler : IQueryHandler<GetCategoriesQuery, GetCategoriesQueryResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoriesQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetCategoriesQueryResponse> Handle(GetCategoriesQuery query,
            CancellationToken cancellationToken)
        {
            if (query.PageNumber < 1)
            {
                throw new ArgumentException("Page number must be greater than zero.", nameof(query));
            }

            if (query.PageSize < 1 || query.PageSize > 100)
            {
                throw new ArgumentException("Page size must be between 1 and 100.", nameof(query));
            }

            CategoryQuerySpecification spec = new(query.RootCategoriesOnly, query.ActiveOnly);
            PaginatedResult<Category> result = await _unitOfWork.GetPaginatedBySpecAsync<Category>(
                spec, query.PageNumber, query.PageSize, cancellationToken);

            List<CategoryDto> categoryDtos = result.Items.Select(c => new CategoryDto(
                c.Id,
                c.Name,
                c.Description,
                c.ParentCategoryId,
                c.IsActive,
                c.IsRootCategory())).ToList();

            return new GetCategoriesQueryResponse(categoryDtos, result.TotalCount, query.PageNumber, query.PageSize);
        }
    }
}
