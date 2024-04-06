using BLL.Models;

namespace BLL.Interfaces;

public interface IAccountService
{
    Task<LoginResponse> LoginAsync(LoginRequest loginRequest);
}
