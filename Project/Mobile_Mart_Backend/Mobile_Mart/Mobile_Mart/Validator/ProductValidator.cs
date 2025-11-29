using FluentValidation;
using Mobile_Mart.Models;

namespace Mobile_Mart.Validator
{
    public class ProductValidator : AbstractValidator<Product>
    {
        public ProductValidator()
        {
            RuleFor(x => x.ProductName)
                .NotEmpty().WithMessage("Product name is required.")
                .MaximumLength(100).WithMessage("Product name cannot exceed 100 characters.");

            RuleFor(x => x.BrandId)
                .NotNull().WithMessage("BrandId is required.")
                  .GreaterThan(0).WithMessage("Brand ID must be greater than 0.");

            RuleFor(x => x.CategoryId)
                .NotNull().WithMessage("CategoryId is required.")
                  .GreaterThan(0).WithMessage("Category ID must be greater than 0.");

            RuleFor(x => x.UserId)
                .NotNull().WithMessage("UserId is required.")
                  .GreaterThan(0).WithMessage("User ID must be greater than 0.");

            RuleFor(x => x.Price)
                .NotNull().WithMessage("Price is required.")
                .GreaterThanOrEqualTo(0).WithMessage("Price must be zero or greater.");

            RuleFor(x => x.Description)
                .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.")
                .When(x => !string.IsNullOrEmpty(x.Description));

        }
    }
}
