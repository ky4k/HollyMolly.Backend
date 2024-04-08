using HM.BLL.Models;

namespace HM.BLL.Interfaces;

public interface IUserService
{
    Task<IEnumerable<UserDto>> GetUsersAsync();
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<OperationResult<UserDto>> ChangeUserRolesAsync(string userId, string[] roles);
    Task<OperationResult> DeleteUserAsync(string userId);
    Task<IEnumerable<string>> GetAllRolesAsync();
}
