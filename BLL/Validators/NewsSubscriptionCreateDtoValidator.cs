using FluentValidation;
using HM.BLL.Models.NewsSubscriptions;

namespace HM.BLL.Validators;

public class NewsSubscriptionCreateDtoValidator : AbstractValidator<NewsSubscriptionCreateDto>
{
    public NewsSubscriptionCreateDtoValidator()
    {
        RuleFor(nsd => nsd.Email)
            .ApplyEmailValidationRules();
    }
}
