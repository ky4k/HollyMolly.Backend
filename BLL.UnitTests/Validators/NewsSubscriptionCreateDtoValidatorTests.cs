using FluentValidation.TestHelper;
using HM.BLL.Models.NewsSubscriptions;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class NewsSubscriptionCreateDtoValidatorTests
{
    public NewsSubscriptionCreateDtoValidator _validator;
    public NewsSubscriptionCreateDtoValidatorTests()
    {
        _validator = new NewsSubscriptionCreateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidNewsSubscriptions), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(NewsSubscriptionCreateDto model)
    {
        TestValidationResult<NewsSubscriptionCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidNewsSubscriptions), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(NewsSubscriptionCreateDto model)
    {
        TestValidationResult<NewsSubscriptionCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
