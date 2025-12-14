using Catalog.Application.Features.Product.Commands;
using FluentValidation;

namespace Catalog.Application.Features.Product.Validators;

/// <summary>
/// Validator for DeleteProductCommand.
/// Ensures business rules are enforced on command input.
/// </summary>
public class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty()
            .WithMessage("Product ID is required.");
    }
}
