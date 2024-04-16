using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class SubscribersController : ControllerBase
{
    [Route("")]
    [HttpGet]
    public async Task<ActionResult> GetAllSubscribers()
    {
        throw new NotImplementedException();
    }

    [Route("")]
    [HttpPost]
    public async Task<ActionResult> AddSubscriber()
    {
        throw new NotImplementedException();
    }

    [Route("{email}")]
    [HttpDelete]
    public async Task<ActionResult> DeleteSubscriber(string email)
    {
        throw new NotImplementedException();
    }

    [Route("send")]
    [HttpPost]
    public async Task<ActionResult> SendNewsToAllSubscribers()
    {
        throw new NotImplementedException();
    }
}
