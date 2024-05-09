using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Supports;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace WebAPI.UnitTests.Controllers;

public class SupportControllerTests
{
    private readonly ISupportService _supportService;
    private readonly IEmailService _emailService;
    private readonly SupportController _supportController;
    public SupportControllerTests()
    {
        _supportService = Substitute.For<ISupportService>();
        _emailService = Substitute.For<IEmailService>();
        _supportController = new SupportController(_supportService, _emailService);
    }
    [Fact]
    public async Task CreateSupportRequest_ShouldReturnNoContent_WhenSucceeded()
    {
        _supportService.SaveSupportRequestAsync(Arg.Any<SupportCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));
        _emailService.SendSupportEmailAsync(Arg.Any<SupportCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _supportController
            .CreateSupportRequest(new SupportCreateDto(), CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task CreateSupportRequest_ShouldReturnBadRequest_WhenFailedToSave()
    {
        _supportService.SaveSupportRequestAsync(Arg.Any<SupportCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));
        _emailService.SendSupportEmailAsync(Arg.Any<SupportCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _supportController
            .CreateSupportRequest(new SupportCreateDto(), CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task CreateSupportRequest_ShouldReturnBadRequest_WhenFailedToSendEmail()
    {
        _supportService.SaveSupportRequestAsync(Arg.Any<SupportCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));
        _emailService.SendSupportEmailAsync(Arg.Any<SupportCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _supportController
            .CreateSupportRequest(new SupportCreateDto(), CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
}
