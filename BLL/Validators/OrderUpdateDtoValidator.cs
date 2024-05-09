using FluentValidation;
using HM.BLL.Models.Orders;
using HM.DAL.Constants;
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
            .Must(OrderStatuses.IsValidStatus)
                .WithMessage("Invalid order status.");

        RuleFor(order => order.Notes)
            .MaximumLength(500)
            .Must(n => string.IsNullOrWhiteSpace(n) || NotesPattern().IsMatch(n))
                .WithMessage("Notes may only contain Latin or Ukrainian letters, numbers, spaces, and special characters ( ! # $ % & \" / ? , . - _ )");
    }
}
