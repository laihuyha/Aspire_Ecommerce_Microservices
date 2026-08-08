using Basket.Application.Commands;
using FluentValidation;

namespace Basket.Application.Validators
{
    public class SaveCartCommandValidator : AbstractValidator<SaveCartCommand>
    {
        public SaveCartCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.Items).NotEmpty();
            RuleForEach(x => x.Items).SetValidator(new SaveCartCommandItemValidator());
        }
    }

    public class SaveCartCommandItemValidator : AbstractValidator<SaveCartCommandItem>
    {
        public SaveCartCommandItemValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.ProductName).NotEmpty();
            RuleFor(x => x.UnitPrice).GreaterThanOrEqualTo(0).LessThanOrEqualTo(999999.99m);
            RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(9999);
        }
    }
}
