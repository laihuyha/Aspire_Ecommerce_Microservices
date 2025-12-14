using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Application.Features.Category.Exceptions;
using Marten;

namespace Catalog.Application.Features.Category.Commands;

public record DeleteCategoryCommand(Guid Id) : ICommand<DeleteCategoryCommandResponse>;

public record DeleteCategoryCommandResponse(bool Success);

public class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand, DeleteCategoryCommandResponse>
{
    private readonly IDocumentSession _documentSession;

    public DeleteCategoryCommandHandler(IDocumentSession documentSession)
    {
        _documentSession = documentSession;
    }

    public async Task<DeleteCategoryCommandResponse> Handle(DeleteCategoryCommand command, CancellationToken cancellationToken)
    {
        var category = await _documentSession.LoadAsync<Domain.Aggregates.Category.Category>(command.Id, cancellationToken);

        if (category is null)
        {
            throw new CategoryNotFoundException(command.Id);
        }

        _documentSession.Delete(category);
        await _documentSession.SaveChangesAsync(cancellationToken);

        return new DeleteCategoryCommandResponse(true);
    }
}
