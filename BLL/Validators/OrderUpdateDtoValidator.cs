using FluentValidation;
using HM.BLL.Models.Orders;
using System.Text.RegularExpressions;

namespace HM.BLL.Validators;

public partial class OrderUpdateDtoValidator : AbstractValidator<OrderUpdateDto>
{
    [GeneratedRegex(@"^[\p{L}0-9\s!#$%&""/?,.\-_();:']+$")]
    private static partial Regex NotesPattern();
    public OrderUpdateDtoValidator()
    {
        RuleFor(order => order.Status)
            .NotEmpty()
            .Must(BeAValidStatus)
                .WithMessage("Invalid order status.");

        RuleFor(order => order.Notes)
            .MaximumLength(500)
            .Must(n => string.IsNullOrWhiteSpace(n) || NotesPattern().IsMatch(n))
                .WithMessage("Notes may only contain Latin or Ukrainian letters, numbers, spaces, and special characters ( ! # $ % & \" / ? , . - _ )");
    }
    private bool BeAValidStatus(string status)
    {
        var validStatuses = new[]
        {
            "Created",
            "Payment Received",
            "Processing",
            "Shipped",
            "Delivered",
            "Cancelled"
        };
        return validStatuses.Contains(status);
    }
}
