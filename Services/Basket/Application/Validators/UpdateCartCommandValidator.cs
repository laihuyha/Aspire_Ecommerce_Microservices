using Basket.Application.Commands;
using FluentValidation;

namespace Basket.Application.Validators
{
    public class UpdateCartCommandValidator : AbstractValidator<UpdateCartCommand>
    {
        public UpdateCartCommandValidator()
        {
            RuleFor(x => x.UserId).NotEmpty();
            RuleFor(x => x.CartId).NotEmpty();
            RuleForEach(x => x.Items).SetValidator(new UpdateCartCommandItemValidator());
        }
    }

    public class UpdateCartCommandItemValidator : AbstractValidator<UpdateCartCommandItem>
    {
        public UpdateCartCommandItemValidator()
        {
            RuleFor(x => x.ProductId).NotEmpty();
            RuleFor(x => x.Quantity).GreaterThanOrEqualTo(0).LessThanOrEqualTo(9999);
        }
    }
}
