using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BuildingBlocks.CQRS;
using BuildingBlocks.Errors;
using Catalog.Domain.Aggregates.Product;
using Catalog.Domain.Aggregates.Product.Events;
using Catalog.Domain.Entities;
using Catalog.Domain.Interfaces;
using Catalog.Domain.Specifications;
using Catalog.Domain.ValueObjects;
using Marten;
using MediatR;

namespace Catalog.Application.Commands
{
    public record CreateProductCommand(
        string Name,
        string Description,
        string ImageUrl,
        decimal? BasePrice,
        IReadOnlyList<CategoryInfo> Categories = null,
        IReadOnlyList<VariantInfo> Variants = null,
        IReadOnlyList<AttributeInfo> Attributes = null) : ICommand<CreateProductCommandResponse>;

    public record CategoryInfo(
        Guid CategoryId,
        string CategoryName);

    public record VariantInfo(
        string Name,
        string Sku,
        decimal Price,
        int StockQuantity,
        IReadOnlyList<AttributeInfo> Attributes = null);

    public record AttributeInfo(
        string Name,
        string Value);

    public record CreateProductCommandResponse(Guid ProductId);

    public class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateProductCommandResponse>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateProductCommandHandler(IUnitOfWork unitOfWork, IMediator mediator)
        {
            _mediator = mediator;     // Domain events publication
            _unitOfWork = unitOfWork; // Repository access and transaction management
        }

        public async Task<CreateProductCommandResponse> Handle(CreateProductCommand command,
            CancellationToken cancellationToken)
        {
            await ValidateNoDuplicateSkusAsync(command.Variants, cancellationToken);

            Product product = Product.Create(
                command.Name,
                command.Description,
                command.ImageUrl,
                command.BasePrice
            );

            if (command.Categories != null && command.Categories.Any())
            {
                foreach (var categoryInfo in command.Categories)
                {
                    product.AddCategory(categoryInfo.CategoryId, categoryInfo.CategoryName);
                }
            }

            if (command.Variants != null && command.Variants.Any())
            {
                foreach (var variantInfo in command.Variants)
                {
                    var variantAttributes = variantInfo.Attributes?
                        .Select(attr => ProductAttr.Create(attr.Name, attr.Value))
                        .ToList();

                    var variant = Variant.Create(
                        variantInfo.Name,
                        variantInfo.Sku,
                        variantInfo.Price,
                        variantInfo.StockQuantity,
                        variantAttributes
                    );

                    product.AddVariant(variant);
                }
            }

            if (command.Attributes != null && command.Attributes.Any())
            {
                foreach (var attrInfo in command.Attributes)
                {
                    var attribute = ProductAttr.Create(attrInfo.Name, attrInfo.Value);
                    product.AddAttribute(attribute);
                }
            }

            await _unitOfWork.Repository<Product>().AddAsync(product, cancellationToken);

            await _unitOfWork.SaveEntitiesAsync(cancellationToken);

            await _mediator.Publish(new ProductCreatedDomainEvent(product.Id), cancellationToken);

            return new CreateProductCommandResponse(product.Id);
        }

        private async Task ValidateNoDuplicateSkusAsync(IReadOnlyList<VariantInfo> variants,
            CancellationToken cancellationToken)
        {
            if (variants == null || !variants.Any())
            {
                return;
            }

            // Check for duplicate SKUs within the same product
            var skus = variants.Select(v => v.Sku).ToList();
            var uniqueSkus = new HashSet<string>(skus);
            if (skus.Count != uniqueSkus.Count)
            {
                throw new BuildingBlocks.Errors.DomainException("Duplicate SKUs found within product variants");
            }

            // Check for duplicate SKUs across all existing products
            var allSkus = variants.Select(v => v.Sku).ToList();
            var existingProductsWithSkus = await _unitOfWork.Products.ExistSkusAsync(allSkus, cancellationToken);
            if (existingProductsWithSkus)
            {
                var conflictingSkus = await GetConflictingSkusAsync(skus, cancellationToken);

                throw new BuildingBlocks.Errors.DomainException(
                    $"SKUs already exist in other products: {string.Join(", ", conflictingSkus)}");
            }
        }

        private async Task<List<string>> GetConflictingSkusAsync(List<string> skus, CancellationToken cancellationToken)
        {
            var productRepository = _unitOfWork.Repository<Product>();

            // Get all products that have any of the provided SKUs
            var products = await productRepository.GetListAsync(new ProductWithSkusSpecification(skus),
                cancellationToken);

            return products
                .SelectMany(p => p.Variants)
                .Where(v => skus.Contains(v.SKU, StringComparer.OrdinalIgnoreCase))
                .Select(v => v.SKU)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
