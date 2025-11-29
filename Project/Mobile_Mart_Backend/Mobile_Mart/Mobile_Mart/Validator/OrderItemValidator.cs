using FluentValidation;
using Mobile_Mart.Models;

namespace Mobile_Mart.Validator
{
    public class OrderItemValidator : AbstractValidator<OrderItem>
    {
        public OrderItemValidator()
        {
            RuleFor(x => x.OrderId)
                .NotNull().WithMessage("OrderId is required.");

            RuleFor(x => x.ProductId)
                .NotNull().WithMessage("ProductId is required.")
                 .GreaterThan(0).WithMessage("Product ID must be greater than 0.");

            RuleFor(x => x.Quantity)
                .NotNull().WithMessage("Quantity is required.")
                .GreaterThan(0).WithMessage("Quantity must be greater than 0.");

            RuleFor(x => x.Price)
                .NotNull().WithMessage("Price is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or more.");

            RuleFor(x => x.UserId)
                .NotNull().WithMessage("UserId is required.")
                 .GreaterThan(0).WithMessage("User ID must be greater than 0.");

        }
    }
}
