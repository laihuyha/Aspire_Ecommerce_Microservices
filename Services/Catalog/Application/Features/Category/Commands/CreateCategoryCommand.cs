using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Marten;

namespace Catalog.Application.Features.Category.Commands;

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

    public async Task<CreateCategoryCommandResponse> Handle(CreateCategoryCommand command, CancellationToken cancellationToken)
    {
        Domain.Aggregates.Category.Category category;

        if (command.ParentCategoryId.HasValue)
        {
            // Validate parent category exists
            var parentCategory = await _documentSession.LoadAsync<Domain.Aggregates.Category.Category>(
                command.ParentCategoryId.Value, cancellationToken);

            if (parentCategory is null)
            {
                throw new Exceptions.CategoryNotFoundException(command.ParentCategoryId.Value);
            }

            category = Domain.Aggregates.Category.Category.CreateSubCategory(
                command.Name,
                command.ParentCategoryId.Value,
                command.Description);
        }
        else
        {
            category = Domain.Aggregates.Category.Category.CreateRootCategory(
                command.Name,
                command.Description);
        }

        _documentSession.Store(category);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return new CreateCategoryCommandResponse(category.Id);
    }
}
