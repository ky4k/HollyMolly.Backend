using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using System.Security.Claims;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(
    IAccountService accountService,
    IGoogleOAuthService googleOAuthService,
    IEmailService emailService,
    IUserService userService,
    IConfigurationHelper configurationHelper
    ) : ControllerBase
{
    private readonly string _mainPage = configurationHelper.GetConfigurationValue("FrontendUrls:PaymentPage") ?? "";
    /// <summary>
    /// Allows to register a new user.
    /// </summary>
    /// <param name="request">Name, email and password of the user to register.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200"> Indicates that the user was successfully created and returns 
    ///     the user model object.</response>
    /// <response code="400">Indicates that user was not created and returns the error message.</response>
    [Route("registration")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegistrationResponse>> Registration(RegistrationRequest request,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        OperationResult<RegistrationResponse> result = await accountService.RegisterUserAsync(request);
        if (!result.Succeeded || result.Payload == null)
        {
            return BadRequest(result.Message);
        }
        if (sendEmail)
        {
            OperationResult<ConfirmationEmailDto> confirmationEmailResult = await accountService
                .GetConfirmationEmailKeyAsync(result.Payload.Id);
            if (confirmationEmailResult.Succeeded && confirmationEmailResult.Payload != null)
            {
                await emailService.SendRegistrationResultEmailAsync(result.Payload.Email,
                    confirmationEmailResult.Payload, cancellationToken);
            }
        }
        return Ok(result.Payload);
    }

    /// <summary>
    /// Allows a user to confirm email using token that was sent to email.
    /// </summary>
    /// <param name="userId">Id of the user to confirm email.</param>
    /// <param name="confirmationEmailToken">A token that was send to the user email.</param>
    /// <response code="200"> Indicates that the email was successfully confirmed.</response>
    /// <response code="400">Indicates that email was not confirmed and returns the error message.</response>
    [Route("{userId}/confirmEmail")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ConfirmEmail(string userId, string confirmationEmailToken)
    {
        OperationResult result = await accountService.ConfirmEmailAsync(userId, confirmationEmailToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows the user to login to the site.
    /// </summary>
    /// <param name="request">User name/email and password</param>
    /// <response code="200">Indicates that the login was successful and returns 
    ///     access token, refresh token and user information.</response>
    /// <response code="400">Indicates that the login has failed and returns the error message.</response>
    [Route("login")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        OperationResult<LoginResponse> result = await accountService.LoginAsync(request);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows to get link to login on the site via Google account.
    /// </summary>
    /// <response code="200">Returns link to the Google login form.</response>
    [Route("login/google")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult<LinkDto> RedirectOnGoogleOAuthServer()
    {
        string redirectUrl = $"https://{Request.Host}{Request.PathBase}/api/account/login/google/getToken";
        string url = googleOAuthService.GenerateOAuthRequestUrl(redirectUrl);
        return Ok(new LinkDto { RedirectToUrl = url });
    }

    /// <summary>
    /// Allows to exchange the code received from Google Api for the user access token.
    /// </summary>
    /// <param name="code">The user code that can be exchanged on the user token.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="302">Redirect user to the frontend page and set token as query parameter.</response>
    /// <response code="400">Indicates that the login has failed and returns the error message.</response>
    [Route("login/google/getToken")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status302Found)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> GetGoogleToken(string code, CancellationToken cancellationToken)
    {
        string googleRedirectUrl = $"https://{Request.Host}{Request.PathBase}/api/account/login/google/getToken";
        string? token = await googleOAuthService.ExchangeCodeOnTokenAsync(code, googleRedirectUrl, cancellationToken);
        if (token == null)
        {
            return BadRequest("Google does not return a valid user token.");
        }
        string? email = await googleOAuthService.GetUserEmailAsync(token, cancellationToken);
        if (email == null)
        {
            return BadRequest("Google does not return a valid user email address.");
        }

        OperationResult result = await accountService.RegisterOidcUserAsync(email);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        OperationResult<string> tokenResult = await accountService.GetOidcTokenAsync(email);
        if (tokenResult.Payload == null)
        {
            return BadRequest(result.Message);
        }
        Dictionary<string, string?> parameters = new()
        {
            { "token", tokenResult.Payload }
        };
        string redirectToFrontendUrl = QueryHelpers.AddQueryString(_mainPage, parameters);
        return Redirect(redirectToFrontendUrl);
    }

    /// <summary>
    /// Allows the user to login to the site using provided by the application token.
    /// </summary>
    /// <param name="loginRequest">The user token provided by the application.</param>
    /// <response code="200">Indicates that the login was successful and returns access token, refresh token and user information.</response>
    /// <response code="400">Indicates that the login has failed and returns the error message.</response>
    [Route("login/google")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> LoginViaToken(LoginOidcRequest loginRequest)
    {
        OperationResult<LoginResponse> result = await accountService
            .LoginOidcUserAsync(loginRequest.Token);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows to exchange expired access token and valid refresh token on the new pair of access and refresh tokens.
    /// </summary>
    /// <param name="tokens">Access and refresh tokens to exchange</param>
    /// <response code="200">Indicates that the operation was successful and returns access token, refresh token and user information.</response>
    /// <response code="400">Indicates that operation has failed and returns the error message.</response>
    [Route("refresh")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<LoginResponse>> RefreshToken(TokensDto tokens)
    {
        OperationResult<LoginResponse> result = await accountService.RefreshTokenAsync(tokens);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows the user to logout from the site in all their active sessions.
    /// </summary>
    /// <response code="204">Indicates that the logout was successful.</response>
    /// <response code="400">Indicates that the logout failed and returns the error message.</response>
    /// <response code="401">Indicates that the user is unauthenticated.</response>
    [Authorize]
    [Route("logoutAllDevices")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> LogoutAllDevices()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        OperationResult result = await accountService.InvalidateAllPreviousTokensAsync(userId);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows a user to get their profile information.
    /// </summary>
    /// <response code="200">Returns the profile of the user.</response>
    /// <response code="401">Indicates that the user is unauthenticated.</response>
    /// <response code="404">Indicates that the profile was not found.</response>
    [Authorize]
    [Route("profile")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetProfile()
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        UserDto? user = await userService.GetUserByIdAsync(userId);
        return user == null ? NotFound() : Ok(user);
    }

    /// <summary>
    /// Allows a user to update their profile information.
    /// </summary>
    /// <param name="profile">Updated profile information.</param>
    /// <response code="200">Indicates that the profile has been updated and returns updated user.</response>
    /// <response code="400">Indicates that the profile has not been updated and returns the error message.</response>
    /// <response code="401">Indicates that the user is unauthenticated and therefore cannot update profile.</response>
    [Authorize]
    [Route("profile")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserDto>> UpdateProfile(ProfileUpdateDto profile)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        OperationResult<UserDto> result = await accountService.UpdateUserProfileAsync(userId, profile);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows a user to update their email.
    /// </summary>
    /// <param name="updatedEmail">Updated email information.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the email has been updated.</response>
    /// <response code="400">Indicates that the email has not been updated and returns the error message.</response>
    /// <response code="401">Indicates that the user is unauthenticated.</response>
    [Authorize]
    [Route("profile/email")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> UpdateEmail(EmailDto updatedEmail,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        string? oldEmail = User.FindFirst(ClaimTypes.Email)?.Value;
        OperationResult result = await accountService.UpdateEmailAsync(userId, updatedEmail.Email);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        if (sendEmail)
        {
            if (oldEmail != null)
            {
                await emailService.SendEmailChangedEmailAsync(oldEmail, cancellationToken);
            }
            OperationResult<ConfirmationEmailDto> confirmationEmailResult = await accountService
                .GetConfirmationEmailKeyAsync(userId);
            if (confirmationEmailResult.Payload != null)
            {
                await emailService.SendRegistrationResultEmailAsync(updatedEmail.Email,
                    confirmationEmailResult.Payload, cancellationToken);
            }
        }
        return NoContent();
    }

    /// <summary>
    /// Allows a user to change their password.
    /// </summary>
    /// <param name="passwords">The current and the new passwords.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the password has been changed.</response>
    /// <response code="400">Indicates that the password has not been updated and returns the error message.</response>
    /// <response code="401">Indicates that the user is unauthenticated and therefore cannot change the password.</response>
    [Authorize]
    [Route("profile/password")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangeUserPassword(ChangePasswordDto passwords,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
        {
            return Unauthorized();
        }
        OperationResult<ResetPasswordTokenDto> result = await accountService.ChangePasswordAsync(userId, passwords);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        if (sendEmail)
        {
            string? email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email != null)
            {
                await emailService.SendPasswordChangedEmailAsync(email, result.Payload!, cancellationToken);
            }
        }
        return NoContent();
    }

    /// <summary>
    /// Allows user to get email with link to reset password in case they have it forgotten.
    /// </summary>
    /// <param name="sendTo">Email of the user to send link to.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the email has been sent.</response>
    /// <response code="400">Indicates that the reset password email cannot be sent and returns the error message.</response>
    [Route("forgetPassword")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendForgetPasswordEmail(EmailDto sendTo,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        OperationResult<ResetPasswordTokenDto> result =
            await accountService.CreatePasswordResetKeyAsync(sendTo.Email);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        if (sendEmail)
        {
            await emailService.SendForgetPasswordEmailAsync(sendTo.Email, result.Payload!, cancellationToken);
        }
        return NoContent();
    }

    /// <summary>
    /// Allows a user to reset password.
    /// </summary>
    /// <param name="userId">Id of the user to reset the password.</param>
    /// <param name="resetPassword">The reset token that was set to the user email and the new password.</param>
    /// <response code="204">Indicates that the password has been changed.</response>
    /// <response code="400">Indicates that the password has not been updated and returns the error message.</response>
    [Route("{userId}/password/reset")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ResetPassword(string userId, ResetPasswordDto resetPassword)
    {
        OperationResult<UserDto> result = await accountService.ResetPasswordAsync(userId, resetPassword);
        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        return NoContent();
    }
}
