using Azure;
using HM.BLL.Interfaces;
using HM.BLL.Models;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(
    IAccountService accountService
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
    public async Task<ActionResult> Registration(RegistrationRequest request)
    {
        var response = await accountService.RegisterUserAsync(request);
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
        var response = await accountService.LoginAsync(request);
        return response.Succeeded ? Ok(response.Payload) : BadRequest(response.Message);
    }

    [Route("{userId}")]
    [HttpPut]
    public async Task<ActionResult<UserDto>> UpdateProfile(string userId)
    {
        throw new NotImplementedException();
    }
}
