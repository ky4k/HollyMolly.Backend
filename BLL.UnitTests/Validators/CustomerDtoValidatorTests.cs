using FluentValidation.TestHelper;
using HM.BLL.Interfaces;
using HM.BLL.Models.Orders;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;
using NSubstitute;

namespace HM.BLL.UnitTests.Validators;

public class CustomerDtoValidatorTests
{
    private readonly INewPostService _newPostService;
    private readonly CustomerDtoValidator _validator;
    public CustomerDtoValidatorTests()
    {
        _newPostService = Substitute.For<INewPostService>();
        _newPostService.CheckIfCityIsValidAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _newPostService.CheckIfAddressIsValidAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _validator = new CustomerDtoValidator(_newPostService);
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidCustomers), MemberType = typeof(ValidationData))]
    public async Task Validation_ShouldSucceed_WhenAllFieldsAreValid(CustomerDto customer)
    {
        TestValidationResult<CustomerDto> result = await _validator.TestValidateAsync(customer);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidCustomers), MemberType = typeof(ValidationData))]
    public async Task Validation_ShouldFail_WhenAnyFieldIsInvalid(CustomerDto customer)
    {
        TestValidationResult<CustomerDto> result = await _validator.TestValidateAsync(customer);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
