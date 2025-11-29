using FluentValidation;
using Mobile_Mart.Models;

namespace Mobile_Mart.Validator
{
    public class BrandValidator : AbstractValidator<Brand>
    {
        public BrandValidator()
        {
            RuleFor(x => x.BrandName)
                .NotEmpty().WithMessage("Brand name is required.")
                .MaximumLength(50).WithMessage("Brand name cannot exceed 50 characters.");

            RuleFor(x => x.UserId)
                .NotNull().WithMessage("UserId is required.")
                 .GreaterThan(0).WithMessage("User ID must be greater than 0.");

          
        }
    }

}
