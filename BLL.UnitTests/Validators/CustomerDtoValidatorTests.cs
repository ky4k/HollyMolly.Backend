using FluentValidation.TestHelper;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Orders;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;
using NSubstitute;

namespace HM.BLL.UnitTests.Validators;

public class CustomerDtoValidatorTests
{
    private readonly INewPostCityesService _newPostService;
    private readonly CustomerCreateDtoValidator _validator;
    public CustomerDtoValidatorTests()
    {
        _newPostService = Substitute.For<INewPostCityesService>();
        _newPostService.CheckIfCityIsValidAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _newPostService.CheckIfAddressIsValidAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _validator = new CustomerCreateDtoValidator(_newPostService);
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidCustomers), MemberType = typeof(ValidationData))]
    public async Task Validation_ShouldSucceed_WhenAllFieldsAreValid(CustomerCreateDto customer)
    {
        TestValidationResult<CustomerCreateDto> result = await _validator.TestValidateAsync(customer);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidCustomers), MemberType = typeof(ValidationData))]
    public async Task Validation_ShouldFail_WhenAnyFieldIsInvalid(CustomerCreateDto customer)
    {
        TestValidationResult<CustomerCreateDto> result = await _validator.TestValidateAsync(customer);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
