using HM.BLL.Interfaces;
using HM.BLL.Models.Supports;
using HM.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupportController(ISupportService supportService, IEmailService emailService) : ControllerBase
    {
        /// <summary>
        /// Create new request in support.
        /// </summary>
        /// <param name="supportRequest">Object `SupportDto` with data of request.</param>
        /// <param name="cancellationToken">Cancell token.</param>
        /// <response code="200">Successful creation of a support request.</response>
        /// <response code="400">Validation errors or a save error.</response>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateSupportRequest([FromBody] SupportDto supportRequest, CancellationToken cancellationToken)
        {
            var result = await supportService.SaveSupportRequestAsync(supportRequest, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(result.Message);
            }
            var emailResult = await emailService.SendSupportEmailAsync(supportRequest, cancellationToken);

            if (!emailResult.Succeeded)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, "Support request was saved, but the email could not be sent.");
            }

            return Ok("Запит у службу підтримки створено успішно.");
        }
    }
}
