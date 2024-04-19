using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class NewsSubscriptionCreateDtoValidator : AbstractValidator<NewsSubscriptionCreateDto>
{
    public NewsSubscriptionCreateDtoValidator()
    {
        RuleFor(nsd => nsd.Email)
            .ApplyEmailValidationRules();
    }
}
