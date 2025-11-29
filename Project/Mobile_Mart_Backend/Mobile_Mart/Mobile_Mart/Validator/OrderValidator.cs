using FluentValidation;
using Mobile_Mart.Models;

namespace Mobile_Mart.Validator
{
    public class OrderValidator : AbstractValidator<Order>
    {
        public OrderValidator()
        {
            RuleFor(x => x.UserId)
                .NotNull().WithMessage("UserId is required.")
                  .GreaterThan(0).WithMessage("User ID must be greater than 0.");

         
            RuleFor(x => x.TotalAmount)
                .NotNull().WithMessage("TotalAmount is required.")
                .GreaterThanOrEqualTo(0).WithMessage("TotalAmount must be zero or more.");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Status is required.");
        }
    }
}
