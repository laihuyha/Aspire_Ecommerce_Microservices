using System;
using Catalog.Application.Commands;
using FluentValidation;

namespace Catalog.Application.Validators
{
    /// <summary>
    ///     Validator for CreateProductCommand.
    ///     Ensures business rules are enforced on command input.
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

            RuleFor(x => x.BasePrice)
                .GreaterThanOrEqualTo(0)
                .When(x => x.BasePrice.HasValue)
                .WithMessage("Base price must be greater than or equal to zero.");

            // Categories validation
            RuleForEach(x => x.Categories)
                .ChildRules(category =>
                {
                    category.RuleFor(c => c.CategoryId)
                        .NotEmpty()
                        .WithMessage("Category ID is required.");

                    category.RuleFor(c => c.CategoryName)
                        .NotEmpty()
                        .WithMessage("Category name is required.")
                        .MaximumLength(100)
                        .WithMessage("Category name must not exceed 100 characters.");
                })
                .When(x => x.Categories != null);

            RuleFor(x => x.Categories)
                .Must(categories => categories == null || categories.Count <= 10)
                .WithMessage("Product cannot belong to more than 10 categories.");

            // Variants validation
            RuleForEach(x => x.Variants)
                .ChildRules(variant =>
                {
                    variant.RuleFor(v => v.Name)
                        .NotEmpty()
                        .WithMessage("Variant name is required.")
                        .MaximumLength(200)
                        .WithMessage("Variant name must not exceed 200 characters.");

                    variant.RuleFor(v => v.Sku)
                        .NotEmpty()
                        .WithMessage("Variant SKU is required.")
                        .Matches(@"^[a-zA-Z0-9\-_]+$")
                        .WithMessage("SKU can only contain letters, numbers, hyphens, and underscores.")
                        .MaximumLength(50)
                        .WithMessage("SKU must not exceed 50 characters.");

                    variant.RuleFor(v => v.Price)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Variant price must be greater than or equal to zero.")
                        .LessThanOrEqualTo(999999.99m)
                        .WithMessage("Variant price cannot exceed 999,999.99.");

                    variant.RuleFor(v => v.StockQuantity)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Stock quantity must be greater than or equal to zero.")
                        .LessThanOrEqualTo(999999)
                        .WithMessage("Stock quantity cannot exceed 999,999.");

                    // Variant attributes validation
                    variant.RuleForEach(v => v.Attributes)
                        .ChildRules(attr =>
                        {
                            attr.RuleFor(a => a.Name)
                                .NotEmpty()
                                .WithMessage("Attribute name is required.")
                                .MaximumLength(100)
                                .WithMessage("Attribute name must not exceed 100 characters.");

                            attr.RuleFor(a => a.Value)
                                .NotEmpty()
                                .WithMessage("Attribute value is required.")
                                .MaximumLength(500)
                                .WithMessage("Attribute value must not exceed 500 characters.");
                        })
                        .When(v => v.Attributes != null);

                    variant.RuleFor(v => v.Attributes)
                        .Must(attrs => attrs == null || attrs.Count <= 20)
                        .WithMessage("Variant cannot have more than 20 attributes.");
                })
                .When(x => x.Variants != null);

            RuleFor(x => x.Variants)
                .Must(variants => variants == null || variants.Count <= 100)
                .WithMessage("Product cannot have more than 100 variants.");

            // Product attributes validation
            RuleForEach(x => x.Attributes)
                .ChildRules(attr =>
                {
                    attr.RuleFor(a => a.Name)
                        .NotEmpty()
                        .WithMessage("Attribute name is required.")
                        .MaximumLength(100)
                        .WithMessage("Attribute name must not exceed 100 characters.");

                    attr.RuleFor(a => a.Value)
                        .NotEmpty()
                        .WithMessage("Attribute value is required.")
                        .MaximumLength(500)
                        .WithMessage("Attribute value must not exceed 500 characters.");
                })
                .When(x => x.Attributes != null);

            RuleFor(x => x.Attributes)
                .Must(attrs => attrs == null || attrs.Count <= 10)
                .WithMessage("Product cannot have more than 10 attributes.");
        }

        private static bool BeValidUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                return true; // Optional field
            }

            return Uri.TryCreate(url, UriKind.Absolute, out _) &&
                   (url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                    url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
        }
    }
}
