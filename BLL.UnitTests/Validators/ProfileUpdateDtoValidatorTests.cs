using FluentValidation.TestHelper;
using HM.BLL.Interfaces;
using HM.BLL.Models.Users;
using HM.BLL.UnitTests.TestHelpers;
using HM.BLL.Validators;
using NSubstitute;

namespace HM.BLL.UnitTests.Validators;

public class ProfileUpdateDtoValidatorTests
{
    private readonly INewPostService _newPostService;
    private readonly ProfileUpdateDtoValidator _validator;
    public ProfileUpdateDtoValidatorTests()
    {
        _newPostService = Substitute.For<INewPostService>();
        _newPostService.CheckIfAddressIsValidAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _validator = new ProfileUpdateDtoValidator(_newPostService);
    }
    [Theory]
    [MemberData(nameof(ValidationData.ValidProfileUpdates), MemberType = typeof(ValidationData))]
    public async Task Validation_ShouldSucceed_WhenModelIsValid(ProfileUpdateDto model)
    {
        TestValidationResult<ProfileUpdateDto> result = await _validator.TestValidateAsync(model);

        Assert.NotNull(result);
        Assert.True(result.IsValid);
    }
    [Theory]
    [MemberData(nameof(ValidationData.InvalidProfileUpdates), MemberType = typeof(ValidationData))]
    public async Task Validation_ShouldFail_WhenModelIsInvalid(ProfileUpdateDto model)
    {
        TestValidationResult<ProfileUpdateDto> result = await _validator.TestValidateAsync(model);

        Assert.NotNull(result);
        Assert.False(result.IsValid);
    }
}
