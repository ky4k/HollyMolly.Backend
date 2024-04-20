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
        OperationResult<RegistrationResponse> response = await accountService.RegisterUserAsync(request);
        if (!response.Succeeded || response.Payload == null)
        {
            return BadRequest(response.Message);
        }
        if (sendEmail)
        {
            OperationResult<ConfirmationEmailDto> confirmationEmailResult = await accountService
                .GetConfirmationEmailKey(response.Payload.Id);
            if (confirmationEmailResult.Succeeded && confirmationEmailResult.Payload != null)
            {
                await emailService.SendRegistrationResultEmailAsync(response.Payload.Email,
                    confirmationEmailResult.Payload, cancellationToken);
            }
        }
        return Ok(response.Payload);
    }

    /// <summary>
    /// Allows a user to confirm email using token that was sent to email.
    /// </summary>
    /// <param name="userId">Id of the user to confirm email.</param>
    /// <param name="confirmationEmailToken">A token that was send to the user email.</param>
    /// <response code="200"> Indicates that the user was successfully created and returns 
    ///     the user model object.</response>
    /// <response code="400">Indicates that user was not created and returns the error message.</response>
    [Route("{userId}/confirmEmail")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegistrationResponse>> ConfirmEmail(string userId, string confirmationEmailToken)
    {
        OperationResult response = await accountService.ConfirmEmailAsync(userId, confirmationEmailToken);
        return response.Succeeded ? NoContent() : BadRequest(response.Message);
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
    public ActionResult<LinkDto> RedirectOnGoogleOAuthServer()
    {
        string redirectUrl = $"https://{Request.Host}{Request.PathBase}/api/account/login/google/getToken";
        var url = googleOAuthService.GenerateOAuthRequestUrl(redirectUrl);
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

        OperationResult response = await accountService.RegisterOidcUserAsync(email);
        if (!response.Succeeded)
        {
            return BadRequest(response.Message);
        }
        string? oidcToken = (await accountService.GetOidcTokenAsync(email)).Payload;

        string redirectToFrontendUrl = $"https://holly-molly.vercel.app/?token={oidcToken}";
        return Redirect(redirectToFrontendUrl);
    }

    /// <summary>
    /// Allows the user to login to the site using provided by the application token.
    /// </summary>
    /// <param name="loginRequest">The user token provided by the application.</param>
    /// <response code="200">Indicates that the login was successful and returns an access token and user information.</response>
    /// <response code="400">Indicates that the login has failed and returns the error message.</response>
    [Route("login/google")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> LoginViaToken(LoginOidcRequest loginRequest)
    {
        OperationResult<LoginResponse> response = await accountService
            .LoginOidcUserAsync(loginRequest.Token);
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
    /// <param name="userId">Id of the user to change password.</param>
    /// <param name="passwords">The current and the new passwords.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
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
    public async Task<ActionResult> ChangeUserPassword(string userId, ChangePasswordDto passwords,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        if (User.FindFirst(ClaimTypes.NameIdentifier)?.Value != userId)
        {
            return Forbid();
        }
        OperationResult<ResetPasswordTokenDto> response = await accountService.ChangePasswordAsync(userId, passwords);
        if (!response.Succeeded || response.Payload == null)
        {
            return BadRequest(response.Message);
        }
        if (sendEmail)
        {
            string? email = User.FindFirst(ClaimTypes.Email)?.Value;
            if (email != null)
            {
                await emailService.SendPasswordChangedEmail(email, response.Payload, cancellationToken);
            }
        }
        return NoContent();
    }

    /// <summary>
    /// Allows user to get email with link to reset password in case they have it forgotten.
    /// </summary>
    /// <param name="email">Email of the user to send link.</param>
    /// <param name="sendEmail">A real email will be sent only if this parameter is set to true.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the email has been sent.</response>
    /// <response code="400">Indicates that the reset password email cannot be sent and returns the error message.</response>
    [Route("/forgetPassword")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> SendForgetPasswordEmail(string email,
        CancellationToken cancellationToken, bool sendEmail = false)
    {
        OperationResult<ResetPasswordTokenDto> response = await accountService
            .CreatePasswordResetKeyAsync(email);
        if (!response.Succeeded || response.Payload == null)
        {
            return BadRequest(response.Message);
        }
        if (sendEmail)
        {
            await emailService.SendForgetPasswordEmailAsync(email, response.Payload, cancellationToken);
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
        OperationResult<UserDto> response = await accountService.ResetPasswordAsync(userId, resetPassword);
        if (!response.Succeeded || response.Payload == null)
        {
            return BadRequest(response.Message);
        }
        return NoContent();
    }
}
