using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TestController : ControllerBase
{
    /// <summary>
    /// Allows to test API accessibility for any user.
    /// </summary>
    /// <response code="200">Indicates that API is working.</response>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public ActionResult Test()
    {
        return Ok(new { Succeded = true });
    }

    /// <summary>
    /// Allows to test API accessibility for the authenticated users.
    /// </summary>
    /// <response code="200">Indicates that user is authenticated.</response>
    /// <response code="401">Indicates that user is not authenticated.</response>
    [Authorize]
    [Route("authorize")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public ActionResult TestAuthorize()
    {
        return Ok(new { Succeded = true });
    }
}
