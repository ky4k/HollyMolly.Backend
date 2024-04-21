using FluentValidation;
using HM.BLL.Models.Orders;

namespace HM.BLL.Validators;

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator()
    {
        RuleFor(ocd => ocd.Customer)
            .SetValidator(new CustomerDtoValidator());
        RuleForEach(ocd => ocd.OrderRecords)
            .Must(or => or.Quantity > 0)
                .WithMessage("Quantity must be greater than 0.");
    }
}
