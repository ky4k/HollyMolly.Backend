using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HM.BLL.Services;

public class UserService(
    UserManager<User> userManager,
    RoleManager<Role> roleManager
    ) : IUserService
{
    public async Task<IEnumerable<UserDto>> GetUsersAsync()
    {
        List<UserDto> users = [];
        foreach (var user in await userManager.Users.ToListAsync())
        {
            IEnumerable<string> roles = await userManager.GetRolesAsync(user);
            UserDto userDto = user.ToUserDto(roles);
            users.Add(userDto);
        }
        return users;
    }

    public async Task<UserDto?> GetUserByIdAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        UserDto? userDto = null;
        if (user != null)
        {
            IEnumerable<string> roles = await userManager.GetRolesAsync(user);
            userDto = user.ToUserDto(roles);
        }
        return userDto;
    }

    public async Task<OperationResult<UserDto>> ChangeUserRolesAsync(string userId, string[] roles)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult<UserDto>(false, "User with such an id does not exist");
        }
        
        var oldRoles = await userManager.GetRolesAsync(user);
        var removeResult = await userManager.RemoveFromRolesAsync(user, oldRoles);
        var addResult = await userManager.AddToRolesAsync(user, roles);
        
        if(removeResult.Succeeded && addResult.Succeeded)
        {
            IEnumerable<string> newRoles = await userManager.GetRolesAsync(user);
            UserDto userDto = user.ToUserDto(newRoles);
            return new OperationResult<UserDto>(true, userDto);
        }
        else
        {
            string removeErrors = string.Join(" ", removeResult.Errors);
            string addErrors = string.Join(" ", addResult.Errors);
            return new OperationResult<UserDto>(false, removeErrors + " " + addErrors);
        }
    }

    public async Task<OperationResult> DeleteUserAsync(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user == null)
        {
            return new OperationResult(false, "User with such an id does not exist.");
        }
        var result = await userManager.DeleteAsync(user);
        return result.Succeeded
            ? new OperationResult(true)
            : new OperationResult(false, string.Join(" ", result.Errors));
    }

    public async Task<IEnumerable<string>> GetAllRolesAsync()
    {
        return await roleManager.Roles
            .Where(r => r.Name != null)
            .Select(r => r.Name!).ToListAsync();
    }
}
