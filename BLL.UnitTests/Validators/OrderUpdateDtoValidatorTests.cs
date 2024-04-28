using FluentValidation.TestHelper;
using HM.BLL.Models.Orders;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class OrderUpdateDtoValidatorTests
{
    private readonly OrderUpdateDtoValidator _validator;
    public OrderUpdateDtoValidatorTests()
    {
        _validator = new OrderUpdateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidOrdersUpdate), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(OrderUpdateDto model)
    {
        TestValidationResult<OrderUpdateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidOrdersUpdate), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(OrderUpdateDto model)
    {
        TestValidationResult<OrderUpdateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
