using FluentValidation;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Orders;

namespace HM.BLL.Validators;

public class CustomerCreateDtoValidator : AbstractValidator<CustomerCreateDto>
{
    public CustomerCreateDtoValidator(INewPostCityesService newPostService)
    {
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
                .WithMessage("City is required")
            .MustAsync(newPostService.CheckIfCityIsValidAsync)
                .WithMessage("The city name must be in the exact format provided by the New Post.");
        RuleFor(c => c.DeliveryAddress)
            .NotEmpty()
                .WithMessage("Delivery address is required")
            .MustAsync(async (customer, address, cancellation) =>
                await newPostService.CheckIfAddressIsValidAsync(customer.City, address, cancellation))
                .WithMessage("There is no New Post office at the specified address in the city.");
    }
}
