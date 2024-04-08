using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface IAccountService
{
    Task<OperationResult<RegistrationResponse>> RegisterUserAsync(RegistrationRequest request);
    Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest loginRequest);
    Task<OperationResult<UserDto>> UpdateUserProfileAsync(string userId, ProfileUpdateDto profile);
    Task<OperationResult> ChangePasswordAsync(string userId, ChangePasswordDto passwords);
}
