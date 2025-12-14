using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Marten;

namespace Catalog.Application.Features.Category.Queries;

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
    private readonly IQuerySession _querySession;

    public GetCategoriesQueryHandler(IQuerySession querySession)
    {
        _querySession = querySession;
    }

    public async Task<GetCategoriesQueryResponse> Handle(GetCategoriesQuery query, CancellationToken cancellationToken)
    {
        if (query.PageNumber < 1)
        {
            throw new ArgumentException("Page number must be greater than zero.", nameof(query));
        }

        if (query.PageSize < 1 || query.PageSize > 100)
        {
            throw new ArgumentException("Page size must be between 1 and 100.", nameof(query));
        }

        IQueryable<Domain.Aggregates.Category.Category> categoriesQuery = _querySession.Query<Domain.Aggregates.Category.Category>();

        if (query.RootCategoriesOnly == true)
        {
            categoriesQuery = categoriesQuery.Where(c => c.ParentCategoryId == null);
        }

        if (query.ActiveOnly == true)
        {
            categoriesQuery = categoriesQuery.Where(c => c.IsActive);
        }

        var totalCount = await categoriesQuery.CountAsync(cancellationToken);

        var categories = await categoriesQuery
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var categoryDtos = categories.Select(c => new CategoryDto(
            c.Id,
            c.Name,
            c.Description,
            c.ParentCategoryId,
            c.IsActive,
            c.IsRootCategory())).ToList();

        return new GetCategoriesQueryResponse(categoryDtos, totalCount, query.PageNumber, query.PageSize);
    }
}
