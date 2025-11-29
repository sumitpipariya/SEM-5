using FluentValidation;
using Mobile_Mart.Models;

namespace Mobile_Mart.Validator
{
    public class UserModelValidator : AbstractValidator<User>
    {
        public UserModelValidator()
        {
            RuleFor(x => x.FullName)
             .NotEmpty()
             .WithMessage("Full name is required.");

            RuleFor(x => x.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .EmailAddress()
                .WithMessage("Email format is invalid.");

            RuleFor(x => x.Password)
                .NotEmpty()
                .WithMessage("Password is required.");

            RuleFor(x => x.Phone)
                .NotEmpty()
                .WithMessage("Phone is required.")
                .Matches(@"^\d{10}$")
                .WithMessage("Phone must be 10 digits.");

            RuleFor(x => x.Role)
                .NotEmpty();

            RuleFor(x => x.AddressLine)
                .NotEmpty()
                .WithMessage("Address is required.");

            RuleFor(x => x.City)
                .NotEmpty()
                .WithMessage("City is required.");

            RuleFor(x => x.State)
                .NotEmpty()
                .WithMessage("State is required.");
        }
    }
}

