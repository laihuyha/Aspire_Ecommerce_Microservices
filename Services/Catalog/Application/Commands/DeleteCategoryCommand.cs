using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Category;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Commands
{
    public record DeleteCategoryCommand(Guid Id) : ICommand<DeleteCategoryCommandResponse>;

    public record DeleteCategoryCommandResponse(bool Success);

    public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, DeleteCategoryCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<DeleteCategoryCommandResponse> Handle(DeleteCategoryCommand command,
            CancellationToken cancellationToken)
        {
            Category category = await _unitOfWork.GetByIdAsync<Category>(command.Id, cancellationToken);

            if (category is null)
            {
                throw new CategoryNotFoundException(command.Id);
            }

            await _unitOfWork.Repository<Category>().DeleteAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new DeleteCategoryCommandResponse(true);
        }
    }
}
