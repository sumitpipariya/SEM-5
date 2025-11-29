using FluentValidation;
using Mobile_Mart.Models;

namespace Mobile_Mart.Validator
{
    public class CartValidator : AbstractValidator<Cart>
    {
        public CartValidator()
        {
            RuleFor(x => x.UserId)
                  .NotNull().WithMessage("UserId is required.")
                   .GreaterThan(0).WithMessage("User ID must be greater than 0.");

            RuleFor(x => x.ProductId)
                .NotNull().WithMessage("ProductId is required.")
                  .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

            RuleFor(x => x.Quantity)
                .NotNull().WithMessage("Quantity is required.")
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

            
        }
    }
}
