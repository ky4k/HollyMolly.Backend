using FluentValidation;
using HM.BLL.Interfaces;
using HM.BLL.Models.Orders;

namespace HM.BLL.Validators;

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator(INewPostService newPostService)
    {
        RuleFor(ocd => ocd.Customer)
            .SetValidator(new CustomerDtoValidator(newPostService));
        RuleForEach(ocd => ocd.OrderRecords)
            .Must(or => or.Quantity > 0)
                .WithMessage("Quantity must be greater than 0.");
    }
}
