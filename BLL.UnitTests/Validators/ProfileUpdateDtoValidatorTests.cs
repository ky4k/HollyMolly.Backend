using FluentValidation.TestHelper;
using HM.BLL.Models.Users;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class ProfileUpdateDtoValidatorTests
{
    private readonly ProfileUpdateDtoValidator _validator;
    public ProfileUpdateDtoValidatorTests()
    {
        _validator = new ProfileUpdateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidProfileUpdates), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenModelIsValid(ProfileUpdateDto model)
    {
        TestValidationResult<ProfileUpdateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidProfileUpdates), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenModelIsInvalid(ProfileUpdateDto model)
    {
        TestValidationResult<ProfileUpdateDto> result = _validator.TestValidate(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
