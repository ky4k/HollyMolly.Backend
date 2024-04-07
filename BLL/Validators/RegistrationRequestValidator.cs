using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class RegistrationRequestValidator : AbstractValidator<RegistrationRequest>
{
    private const int MinRecipientNameLength = 1;
    private const int MaxRecipientNameLength = 50;
    private const int MinDomainNameLength = 4;
    private const int MaxDomainNameLength = 50;

    private readonly string incorrectEmailLengthMessage = "Incorrect length of the email part(s): ensure that" +
        $" the recipient name is between {MinRecipientNameLength} and {MaxRecipientNameLength} characters long" +
        $" and the domain name is between {MinDomainNameLength} and {MaxDomainNameLength} characters long.";

    public RegistrationRequestValidator()
    {
        RuleFor(rr => rr.Email)
            .NotEmpty()
                .WithMessage("Email is required.")
            .Matches(@"^[A-Za-z0-9-_.+]*@[A-Za-z0-9]{1}[A-Za-z0-9-.]*\.[A-Za-z0-9]{2,}$")
                .WithMessage("Email is in an invalid format.")
            .Must(ValidateEmailPartsLength)
                .WithMessage(incorrectEmailLengthMessage);
        RuleFor(rr => rr.Password)
            .ApplyPasswordValidationRules();
    }

    private bool ValidateEmailPartsLength(string email)
    {
        var parts = email.Split('@');
        if (parts.Length == 2)
        {
            bool isRecipientNameOfValidLength = parts[0].Length >= MinRecipientNameLength
                && parts[0].Length <= MaxRecipientNameLength;
            bool isDomainNameOfValidLength = parts[1].Length >= MinDomainNameLength
                && parts[1].Length <= MaxDomainNameLength;
            return isDomainNameOfValidLength && isRecipientNameOfValidLength;
        }
        else
        {
            return false;
        }
    }
}
