using FluentValidation.TestHelper;
using HM.BLL.Models.Supports;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class SupportCreateDtoValidatorTests
{
    private readonly SupportCreateDtoValidator _validator;
    public SupportCreateDtoValidatorTests()
    {
        _validator = new SupportCreateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidSupportCreateDtos), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(SupportCreateDto model)
    {
        TestValidationResult<SupportCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidSupportCreateDtos), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(SupportCreateDto model)
    {
        TestValidationResult<SupportCreateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
