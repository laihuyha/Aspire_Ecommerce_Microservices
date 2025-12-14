using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Exceptions;
using Catalog.Domain.Aggregates.Category;
using Marten;

namespace Catalog.Application.Commands
{
    public record CreateCategoryCommand(
        string Name,
        string Description,
        Guid? ParentCategoryId) : ICommand<CreateCategoryCommandResponse>;

    public record CreateCategoryCommandResponse(Guid CategoryId);

    public class CreateCategoryCommandHandler : ICommandHandler<CreateCategoryCommand, CreateCategoryCommandResponse>
    {
        private readonly IDocumentSession _documentSession;

        public CreateCategoryCommandHandler(IDocumentSession documentSession)
        {
            _documentSession = documentSession;
        }

        public async Task<CreateCategoryCommandResponse> Handle(CreateCategoryCommand command,
            CancellationToken cancellationToken)
        {
            Category category;

            if (command.ParentCategoryId.HasValue)
            {
                // Validate parent category exists
                Category parentCategory = await _documentSession.LoadAsync<Category>(
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

            _documentSession.Store(category);
            await _documentSession.SaveChangesAsync(cancellationToken);

            return new CreateCategoryCommandResponse(category.Id);
        }
    }
}
