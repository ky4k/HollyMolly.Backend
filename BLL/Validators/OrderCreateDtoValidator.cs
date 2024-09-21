using FluentValidation;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Orders;

namespace HM.BLL.Validators;

public class OrderCreateDtoValidator : AbstractValidator<OrderCreateDto>
{
    public OrderCreateDtoValidator(INewPostCitiesService newPostService)
    {
        RuleFor(ocd => ocd.Customer)
            .SetValidator(new CustomerCreateDtoValidator(newPostService));
        RuleForEach(ocd => ocd.OrderRecords)
            .Must(or => or.Quantity > 0)
                .WithMessage("Quantity must be greater than 0.");
    }
}
