using FluentValidation.TestHelper;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Orders;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;
using NSubstitute;

namespace HM.BLL.UnitTests.Validators;

public class OrderCreateDtoValidatorTests
{
    private readonly INewPostCityesService _newPostService;
    private readonly OrderCreateDtoValidator _validator;
    public OrderCreateDtoValidatorTests()
    {
        _newPostService = Substitute.For<INewPostCityesService>();
        _newPostService.CheckIfCityIsValidAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _newPostService.CheckIfAddressIsValidAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _validator = new OrderCreateDtoValidator(_newPostService);
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidOrdersCreate), MemberType = typeof(ValidationData))]
    public async Task Validation_ShouldSucceed_WhenModelIsValid(OrderCreateDto model)
    {
        TestValidationResult<OrderCreateDto> result = await _validator.TestValidateAsync(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidOrdersCreate), MemberType = typeof(ValidationData))]
    public async Task Validation_ShouldFail_WhenModelIsInvalid(OrderCreateDto model)
    {
        TestValidationResult<OrderCreateDto> result = await _validator.TestValidateAsync(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
