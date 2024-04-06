using BLL.Interfaces;
using BLL.Models;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController(
    IAccountService accountService
    ) : ControllerBase
{
    [Route("login")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        return Ok(await accountService.LoginAsync(request));
    }
}
