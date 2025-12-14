using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Features.Category.Exceptions;
using Marten;

namespace Catalog.Application.Features.Category.Commands;

public record UpdateCategoryCommand(
    Guid Id,
    string Name,
    string Description) : ICommand<UpdateCategoryCommandResponse>;

public record UpdateCategoryCommandResponse(bool Success);

public class UpdateCategoryCommandHandler : ICommandHandler<UpdateCategoryCommand, UpdateCategoryCommandResponse>
{
    private readonly IDocumentSession _documentSession;

    public UpdateCategoryCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<UpdateCategoryCommandResponse> Handle(UpdateCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _documentSession.LoadAsync<Domain.Aggregates.Category.Category>(command.Id, cancellationToken);

        if (category is null)
        {
            throw new CategoryNotFoundException(command.Id);
        }

        category.UpdateDetails(command.Name, command.Description);

        _documentSession.Store(category);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return new UpdateCategoryCommandResponse(true);
    }
}
