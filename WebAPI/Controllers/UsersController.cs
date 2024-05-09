using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController(
    IUserService userService
    ) : ControllerBase
{
    /// <summary>
    /// Allows an administrator to get a list of all application users.
    /// </summary>
    /// <response code="200">Returns a list of all users.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to get the users list.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}")]
    [Route("")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers(CancellationToken cancellationToken)
    {
        return Ok(await userService.GetUsersAsync(cancellationToken));
    }

    /// <summary>
    /// Allows an administrator, manager, or consultant to retrieve information about a user.
    /// </summary>
    /// <param name="userId">The ID of the user to retrieve information about.</param>
    /// <response code="200">Returns the user with the specified ID.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to retrieve information about the user.</response>
    /// <response code="404">Indicates that no user with the specified ID exists.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator},{DefaultRoles.Manager}")]
    [Route("{userId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetUserById(string userId)
    {
        UserDto? user = await userService.GetUserByIdAsync(userId);
        return user == null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Allows an administrator to change user roles.
    /// </summary>
    /// <param name="userId">The ID of the user whose roles are to be changed.</param>
    /// <param name="roles">The list of roles to assign to the user.</param>
    /// <response code="200">Indicates that the roles have been successfully changed 
    ///     and returns user information.</response>
    /// <response code="400">Indicates that the roles have not been changed and returns an error message.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to change users roles.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}")]
    [Route("{userId}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> ChangeUserRoles(string userId, string[] roles)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == userId
            && !roles.Contains(DefaultRoles.Administrator))
        {
            return BadRequest($"You cannot strip yourself of the role {DefaultRoles.Administrator}. "
                + $"Ask another {DefaultRoles.Administrator} to do so.");
        }
        OperationResult<UserDto> result = await userService.ChangeUserRolesAsync(userId, roles);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows an administrator to delete a user.
    /// </summary>
    /// <param name="userId">The ID of the user to be deleted.</param>
    /// <response code="204">Indicates that the user has been successfully deleted.</response>
    /// <response code="400">Indicates that the user has not been deleted and returns an error message.</response>
    /// <response code="401">Indicates that the endpoint has been called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to delete users.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}")]
    [Route("{userId}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteUser(string userId)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value == userId)
        {
            return BadRequest($"You cannot delete yourself. "
                + $"Ask another {DefaultRoles.Administrator} to do so.");
        }

        OperationResult result = await userService.DeleteUserAsync(userId);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Returns the list of all roles that are available in the application.
    /// </summary>
    /// <response code="200">Returns a list of all roles.</response>
    [Route("roles")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetAllRoles()
    {
        return Ok(await userService.GetAllRolesAsync());
    }
}
