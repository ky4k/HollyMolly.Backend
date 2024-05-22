using FluentValidation;
using HM.BLL.Models.Products;

namespace HM.BLL.Validators;

public class ProductFeedbackCreateDtoValidator : AbstractValidator<ProductFeedbackCreateDto>
{
    public ProductFeedbackCreateDtoValidator()
    {
        RuleFor(feedback => feedback.AuthorName)
            .NotEmpty()
                .WithMessage("Name is required")
            .ApplyNameValidationRules();

        RuleFor(feedback => feedback.Review)
            .NotEmpty()
            .Length(4, 2000)
            .Matches(@"^[\p{L}0-9\s!#$%&""/?,.\-_();:']+$")
                .WithMessage("Review must be 4-2000 characters long and contain only Latin or Ukrainian letters, numbers, spaces, and special characters (!#$%&\"/?.,-_).");

        RuleFor(feedback => feedback.Rating)
            .InclusiveBetween(0, 5)
                .WithMessage("Rating must be between 0 and 5.");
    }
}
