using System;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Domain.Aggregates.Product.Events;
using Marten;
using MediatR;

namespace Catalog.Application.Features.Product.Commands;

public record CreateProductCommand(
    string Name,
    string Description,
    string ImageUrl,
    decimal? BasePrice) : ICommand<CreateProductCommandResponse>;

public record CreateProductCommandResponse(Guid ProductId);

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductCommandResponse>
{
    private readonly IDocumentSession _document;
    private readonly IMediator _mediator;

    public CreateProductCommandHandler(IDocumentSession document, IMediator mediator)
    {
        _document = document;
        _mediator = mediator;
    }

    public async Task<CreateProductCommandResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = Domain.Aggregates.Product.Product.Create(
            command.Name,
            command.Description,
            command.ImageUrl,
            command.BasePrice
        );

        _document.Store(product);
        await _document.SaveChangesAsync(cancellationToken);

        // Publish domain event
        await _mediator.Publish(new ProductCreatedDomainEvent(product.Id), cancellationToken);

        return new CreateProductCommandResponse(product.Id);
    }
}
