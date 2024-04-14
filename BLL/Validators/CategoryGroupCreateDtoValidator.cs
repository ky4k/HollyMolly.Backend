using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class CategoryGroupCreateDtoValidator : AbstractValidator<CategoryGroupCreateDto>
{
    public CategoryGroupCreateDtoValidator()
    {
        RuleFor(cg => cg.Name)
            .NotEmpty()
                .WithMessage("Category name is required.")
            .MinimumLength(3)
                .WithMessage("Category name must be at least 3 characters long.")
            .MaximumLength(50)
                .WithMessage("Category name cannot be longer than 50 characters.");
    }
}
