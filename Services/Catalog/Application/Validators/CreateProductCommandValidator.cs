using System;
using Catalog.Application.Commands;
using FluentValidation;

namespace Catalog.Application.Validators;

/// <summary>
/// Validator for CreateProductCommand.
/// Ensures business rules are enforced on command input.
/// </summary>
public class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
    public CreateProductCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Product name is required.")
            .MaximumLength(200)
            .WithMessage("Product name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000)
            .WithMessage("Description must not exceed 1000 characters.");

        RuleFor(x => x.ImageUrl)
            .Must(BeValidUrl)
            .WithMessage("ImageUrl must be a valid URL.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Categories)
            .NotNull()
            .WithMessage("Categories are required.");
    }

    private bool BeValidUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true; // Optional field

        return Uri.TryCreate(url, UriKind.Absolute, out _) &&
               (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
    }
}
