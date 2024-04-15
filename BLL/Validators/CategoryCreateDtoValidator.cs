using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class CategoryCreateDtoValidator : AbstractValidator<CategoryCreateDto>
{
    public CategoryCreateDtoValidator()
    {
        RuleFor(ccd => ccd.CategoryName)
            .ApplyCategoryValidationRules();
    }
}
