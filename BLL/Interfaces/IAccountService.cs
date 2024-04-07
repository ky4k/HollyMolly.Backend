using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface IAccountService
{
    Task<OperationResult<UserDto>> RegisterUserAsync(RegistrationRequest request);
    Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest loginRequest);
}
