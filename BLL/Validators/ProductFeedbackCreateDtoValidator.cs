using FluentValidation;
using HM.BLL.Models.Products;

namespace HM.BLL.Validators;

public class ProductFeedbackCreateDtoValidator : AbstractValidator<ProductFeedbackCreateDto>
{
    public ProductFeedbackCreateDtoValidator()
    {
        RuleFor(feedback => feedback.AuthorName)
            .NotEmpty()
            .WithMessage("Name is required");

        RuleFor(feedback => feedback.Review)
            .NotEmpty()
            .Length(4, 2000)
            .Matches(@"^[\p{L}0-9\s!#$%&""/?,.\-_();:']+$")
                .WithMessage("Review must be 4-2000 characters long and contain only Latin or Ukrainian letters, numbers, spaces, and special characters (!#$%&\"/?.,-_).");

        RuleFor(feedback => feedback.Rating)
            .InclusiveBetween(-1, 1)
                .WithMessage("Rating must be either Positive (1), Neutral (0), or Negative (-1).");
    }
}
