using FluentValidation.TestHelper;
using HM.BLL.Models.Users;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;

namespace HM.BLL.UnitTests.Validators;

public class EmailUpdateDtoValidatorTests
{
    private readonly EmailUpdateDtoValidator _validator;
    public EmailUpdateDtoValidatorTests()
    {
        _validator = new EmailUpdateDtoValidator();
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidEmailUpdates), MemberType = typeof(ValidationData))]
    public void Validation_ShouldSucceed_WhenAllFieldsAreValid(EmailUpdateDto emailUpdateDto)
    {
        TestValidationResult<EmailUpdateDto> result = _validator.TestValidate(emailUpdateDto);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidEmailUpdates), MemberType = typeof(ValidationData))]
    public void Validation_ShouldFail_WhenAnyFieldIsInvalid(EmailUpdateDto emailUpdateDto)
    {
        TestValidationResult<EmailUpdateDto> result = _validator.TestValidate(emailUpdateDto);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
