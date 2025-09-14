using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using Domain;
using Mapster;

namespace Application.Commands;

public record CreateProductCommand(string Name, List<string> Category, string Description, string ImageUrl, decimal Price) : ICommand<CreateProductCommandResponse>;

public record CreateProductCommandResponse(Guid ProductId);

public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductCommandResponse>
{
    public async Task<CreateProductCommandResponse> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        var product = command.Adapt<Product>();
        await Task.CompletedTask;
        return new CreateProductCommandResponse(product.Id);
    }
}
