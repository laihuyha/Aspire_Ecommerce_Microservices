using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Catalog.Domain.Aggregates.Product.Events;
using Marten;
using MediatR;

namespace Catalog.Application.Commands.Product;

public record CreateProductCommand(
    string Name,
    List<string> Categories,
    string Description,
    string ImageUrl,
    decimal Price) : ICommand<CreateProductCommandResponse>;

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
            command.Price,
            command.Description,
            command.ImageUrl
        );

        foreach (var category in command.Categories ?? [])
        {
            product.AddCategory(category);
        }

        _document.Store(product);
        await _document.SaveChangesAsync(cancellationToken);

        // Publish domain event
        await _mediator.Publish(new ProductCreatedDomainEvent(product.Id), cancellationToken);

        return new CreateProductCommandResponse(product.Id);
    }
}
