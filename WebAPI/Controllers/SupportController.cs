using HM.BLL.Interfaces;
using HM.BLL.Models.Supports;
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
        /// <param name="cancellationToken">Cancellation token.</param>
        /// <response code="204">Indicates that the support request was successfully created.</response>
        /// <response code="400">Indicates that the support request has not been send and returns the error message.</response>
        /// <remarks>
        /// Topic should be a number that represent one of the issue types as follows:
        /// <br/>1 - Account issues
        /// <br/>2 - ProductQuestions
        /// <br/>3 - PaymentQuestions
        /// <br/>0 - Other
        /// </remarks>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> CreateSupportRequest(SupportCreateDto supportRequest, CancellationToken cancellationToken)
        {
            var result = await supportService.SaveSupportRequestAsync(supportRequest, cancellationToken);

            if (!result.Succeeded)
            {
                return BadRequest(result.Message);
            }
            var emailResult = await emailService.SendSupportEmailAsync(supportRequest, cancellationToken);

            if (!emailResult.Succeeded)
            {
                return BadRequest(emailResult.Message);
            }

            return NoContent();
        }
    }
}
