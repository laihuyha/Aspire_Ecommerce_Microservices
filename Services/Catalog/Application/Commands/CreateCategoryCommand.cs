using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Category;
using Catalog.Domain.Interfaces;

namespace Catalog.Application.Commands
{
    public record CreateCategoryCommand(
        string Name,
        string Description,
        Guid? ParentCategoryId) : ICommand<CreateCategoryCommandResponse>;

    public record CreateCategoryCommandResponse(Guid CategoryId);

    public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;

        public CreateCategoryCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateCategoryCommandResponse> Handle(CreateCategoryCommand command,
            CancellationToken cancellationToken)
        {
            Category category;

            if (command.ParentCategoryId.HasValue)
            {
                Category parentCategory = await _unitOfWork.GetByIdAsync<Category>(
                    command.ParentCategoryId.Value, cancellationToken);

                if (parentCategory is null)
                {
                    throw new CategoryNotFoundException(command.ParentCategoryId.Value);
                }

                category = Category.CreateSubCategory(
                    command.Name,
                    command.ParentCategoryId.Value,
                    command.Description);
            }
            else
            {
                category = Category.CreateRootCategory(
                    command.Name,
                    command.Description);
            }

            await _unitOfWork.Repository<Category>().AddAsync(category, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateCategoryCommandResponse(category.Id);
        }
    }
}
