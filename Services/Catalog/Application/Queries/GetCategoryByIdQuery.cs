using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Category;
using Marten;

namespace Catalog.Application.Queries
{
    public record GetCategoryByIdQuery(Guid Id) : IQuery<GetCategoryByIdQueryResponse>;

    public record GetCategoryByIdQueryResponse(
        Guid Id,
        string Name,
        string Description,
        Guid? ParentCategoryId,
        bool IsActive,
        bool IsRootCategory);

    public class GetCategoryByIdQueryHandler : IQueryHandler<GetCategoryByIdQuery, GetCategoryByIdQueryResponse>
    {
        private readonly IQuerySession _querySession;

        public GetCategoryByIdQueryHandler(IQuerySession querySession)
        {
            _querySession = querySession;
        }

        public async Task<GetCategoryByIdQueryResponse> Handle(GetCategoryByIdQuery query,
            CancellationToken cancellationToken)
        {
            Category category = await _querySession.LoadAsync<Category>(query.Id, cancellationToken);

            if (category is null)
            {
                throw new CategoryNotFoundException(query.Id);
            }

            return new GetCategoryByIdQueryResponse(
                category.Id,
                category.Name,
                category.Description,
                category.ParentCategoryId,
                category.IsActive,
                category.IsRootCategory());
        }
    }
}
