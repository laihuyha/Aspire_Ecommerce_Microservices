using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Category;
using Catalog.Domain.Interfaces;

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
        private readonly IUnitOfWork _unitOfWork;

        public GetCategoryByIdQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<GetCategoryByIdQueryResponse> Handle(GetCategoryByIdQuery query,
            CancellationToken cancellationToken)
        {
            Category category = await _unitOfWork.GetByIdAsync<Category>(query.Id, cancellationToken);

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
