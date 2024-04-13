using FluentValidation;
using HM.BLL.Models;

namespace HM.BLL.Validators;

public class OrderUpdateDtoValidator : AbstractValidator<OrderUpdateDto>
{
    public OrderUpdateDtoValidator()
    {
        RuleFor(order => order.Status).NotEmpty().Must(BeAValidStatus).WithMessage("Invalid order status.");

        RuleFor(order => order.Notes)
            .NotEmpty()
            .MaximumLength(500)
            .Matches(@"^[\p{L}0-9\s!#$%&""/?,.\-_]+$")
            .WithMessage("Notes may only contain Latin or Ukrainian letters, numbers, spaces, and special characters ( ! # $ % & “ / ? , . - _ )");
    }
    private bool BeAValidStatus(string status)
    {
        var validStatuses = new[] { "Created", "Payment Received", "Processing", "Shipped", "Delivered", "Cancelled" };
        return validStatuses.Contains(status);
    }
}
