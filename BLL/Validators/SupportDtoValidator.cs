using FluentValidation;
using HM.BLL.Models.Supports;

namespace HM.BLL.Validators;

public class SupportDtoValidator : AbstractValidator<SupportDto>
{
    public SupportDtoValidator()
    {
        RuleFor(x => x.Description)
            .NotNull().WithMessage("Description cannot be null")
            .NotEmpty().WithMessage("Description cannot be empty")
            .Length(4, 500).WithMessage("Description must be between 4 and 500 characters")
            .Matches("^[a-zA-Z0-9а-яА-ЯіІїЇєЄ !#$%&\"/?.,-_();:']+$")
            .WithMessage("Description contains invalid characters");
    }
}
