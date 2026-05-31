using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Category;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Commands
{
    public record UpdateCategoryCommand(
        Guid Id,
        string Name,
        string Description) : ICommand<UpdateCategoryCommandResponse>;

    public record UpdateCategoryCommandResponse(bool Success);

    public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, UpdateCategoryCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public UpdateCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<UpdateCategoryCommandResponse> Handle(UpdateCategoryCommand command,
            CancellationToken cancellationToken)
        {
            Category category = await _unitOfWork.GetByIdAsync<Category>(command.Id, cancellationToken);

            if (category is null)
            {
                throw new CategoryNotFoundException(command.Id);
            }

            category.UpdateDetails(command.Name, command.Description);

            await _unitOfWork.Repository<Category>().UpdateAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new UpdateCategoryCommandResponse(true);
        }
    }
}
