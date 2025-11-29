using FluentValidation;
using Mobile_Mart.Models;

namespace Mobile_Mart.Validator
{
    public class CategoryValidator : AbstractValidator<Category>
    {
        public CategoryValidator()
        {
            RuleFor(x => x.CategoryName)
                .NotEmpty().WithMessage("CategoryName is required.");

            RuleFor(x => x.UserId)
                .NotNull().WithMessage("UserId is required.")
                  .GreaterThan(0).WithMessage("User ID must be greater than 0.");

          
        }
    }
}
