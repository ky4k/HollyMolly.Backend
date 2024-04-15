using HM.BLL.Interfaces;
using HM.BLL.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(
    IAccountService accountService,
    IGoogleOAuthService googleOAuthService,
    IEmailService emailService
    ) : ControllerBase
{
    /// <summary>
    /// Allows to register a new user.
    /// </summary>
    /// <param name="request">Name, email and password of the user to register.</param>
    /// <response code="200"> Indicates that the user was successfully created and returns 
    ///     the user model object.</response>
    /// <response code="400">Indicates that user was not created and returns the error message.</response>
    [Route("registration")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegistrationResponse>> Registration(RegistrationRequest request)
    {
        OperationResult<RegistrationResponse> response = await accountService.RegisterUserAsync(request);
        return response.Succeeded ? Ok(response.Payload) : BadRequest(response.Message);
    }

    /// <summary>
    /// Allows the user to login to the site.
    /// </summary>
    /// <param name="request">User name/email and password</param>
    /// <response code="200">Indicates that the login was successful and returns 
    ///     an access token and user information.</response>
    /// <response code="400">Indicates that the login has failed and returns the error message.</response>
    [Route("login")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        OperationResult<LoginResponse> response = await accountService.LoginAsync(request);
        return response.Succeeded ? Ok(response.Payload) : BadRequest(response.Message);
    }

    /// <summary>
    /// Allows to get link to login on the site via Google account.
    /// </summary>
    /// <response code="200">Returns link to the Google login form.</response>
    [Route("login/google")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult RedirectOnGoogleOAuthServer()
    {
        string redirectUrl = $"https://{Request.Host}{Request.PathBase}/api/account/login/google/getToken";
        var url = googleOAuthService.GenerateOAuthRequestUrl(redirectUrl);
        return Ok(new { RedirectUrl = url });
    }

    /// <summary>
    /// Allows to exchange the code received from Google Api for the user access token.
    /// </summary>
    /// <param name="code">The user code that can be exchanged on the user token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the login was successful and returns 
    ///     an access token and user information.</response>
    /// <response code="400">Indicates that the login has failed and returns the error message.</response>
    [Route("login/google/getToken")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetGoogleToken(string code, CancellationToken cancellationToken)
    {
        string redirectUrl = $"https://{Request.Host}{Request.PathBase}/api/account/login/google/getToken";
        string? token = await googleOAuthService.ExchangeCodeOnTokenAsync(code, redirectUrl, cancellationToken);
        if (token == null)
        {
            return BadRequest("Google does not return a valid user token.");
        }
        string? email = await googleOAuthService.GetUserEmailAsync(token, cancellationToken);
        if (email == null)
        {
            return BadRequest("Google does not return a valid user email address.");
        }

        await accountService.RegisterOidcUserAsync(email);
        OperationResult<LoginResponse> response = await accountService.LoginOidcUserAsync(email);
        return response.Succeeded ? Ok(response.Payload) : BadRequest(response.Message);
    }

    /// <summary>
    /// Allows a user to update the profile information.
    /// </summary>
    /// <param name="userId">Id of the user to update.</param>
    /// <param name="profile">Updated profile information.</param>
    /// <response code="200">Indicates that the profile has been updated and returns updated user.</response>
    /// <response code="400">Indicates that the profile has not been updated and returns the error message.</response>
    /// <response code="401">Indicates that the user is unauthenticated and therefore cannot update profile.</response>
    /// <response code="403">Indicates that the user has no permission to update profile of the user 
    /// with the specified <paramref name="userId"/>.</response>
    [Authorize]
    [Route("{userId}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<UserDto>> UpdateProfile(string userId, ProfileUpdateDto profile)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != userId)
        {
            return Forbid();
        }

        var response = await accountService.UpdateUserProfileAsync(userId, profile);
        return response.Succeeded ? Ok(response.Payload) : BadRequest(response.Message);
    }

    /// <summary>
    /// Allows a user to change password.
    /// </summary>
    /// <param name="userId">Id of the user to update.</param>
    /// <param name="passwords">The current and the new passwords.</param>
    /// <response code="204">Indicates that the password has been changed.</response>
    /// <response code="400">Indicates that the password has not been updated and returns the error message.</response>
    /// <response code="401">Indicates that the user is unauthenticated and therefore cannot change the password.</response>
    /// <response code="403">Indicates that the user has no permission to change password of the user
    /// with the specified <paramref name="userId"/>.</response>
    [Authorize]
    [Route("{userId}/password")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangeUserPassword(string userId, ChangePasswordDto passwords)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != userId)
        {
            return Forbid();
        }
        var response = await accountService.ChangePasswordAsync(userId, passwords);
        return response.Succeeded ? NoContent() : BadRequest(response.Message);
    }
}
