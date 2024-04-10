using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class CustomerDtoValidator : AbstractValidator<CustomerDto>
{
    public CustomerDtoValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty()
                .WithMessage("Email is required")
            .ApplyEmailValidationRules();
        RuleFor(c => c.FirstName)
            .NotEmpty()
                .WithMessage("First name is required")
            .ApplyNameValidationRules();
        RuleFor(c => c.LastName)
            .NotEmpty()
                .WithMessage("Last name is required")
            .ApplyNameValidationRules();
        RuleFor(c => c.PhoneNumber)
            .NotEmpty()
                .WithMessage("Phone number is required")
            .ApplyPhoneNumberValidationRules();
        RuleFor(c => c.City)
            .NotEmpty()
                .WithMessage("City is required");
        RuleFor(c => c.DeliveryAddress)
            .NotEmpty()
                .WithMessage("Delivery address is required");
    }
}
