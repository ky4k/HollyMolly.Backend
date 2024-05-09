using HM.BLL.Models.Common;
using HM.BLL.Models.Users;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class TestControllerTests
{
    private readonly TestController _testController;
    public TestControllerTests()
    {
        _testController = new TestController();
    }
    [Fact]
    public void NavigateToSwagger_ShouldReturnRedirectResult()
    {
        ControllerHelper.MockHost(_testController);
        ActionResult response = _testController.NavigateToSwagger();

        Assert.NotNull(response);
        Assert.IsType<RedirectResult>(response);
    }
    [Fact]
    public void Test_ShouldReturnOkResult()
    {
        ActionResult response = _testController.Test();
        var result = response as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public void TestAuthorize_ShouldReturnOkResult()
    {
        ActionResult response = _testController.TestAuthorize();
        var result = response as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public void ImitateGetRedirectUrl_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_testController);
        ActionResult response = _testController.ImitateGetRedirectUrl("testUrl");
        var result = response as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public void ImitateGoogleRedirect_ShouldReturnRedirectResult()
    {
        ActionResult response = _testController.ImitateGoogleRedirect("testUrl");

        Assert.NotNull(response);
        Assert.IsType<RedirectResult>(response);
    }
    [Fact]
    public void ImitateGoogleLogin_ShouldReturnOkResult_WhenTokenIsValid()
    {
        LoginOidcRequest request = new()
        {
            Token = "1234-5678-9012-3456"
        };
        ActionResult response = _testController.ImitateGoogleLogin(request);
        var result = response as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public void ImitateGoogleLogin_ShouldReturnBadRequest_WhenTokenIsInvalid()
    {
        LoginOidcRequest request = new()
        {
            Token = "Invalid token"
        };

        ActionResult response = _testController.ImitateGoogleLogin(request);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public void CheckoutOrder_ShouldReturnOkResult_WhenImitateSuccess()
    {
        ControllerHelper.MockHost(_testController);

        ActionResult<LinkDto> response = _testController.CheckoutOrder(1, "testUrl", true);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public void CheckoutOrder_ShouldReturnOkResult_WhenImitateFailure()
    {
        ControllerHelper.MockHost(_testController);

        ActionResult<LinkDto> response = _testController.CheckoutOrder(1, "testUrl", false);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public void CheckoutSucceeded_ShouldReturnRedirectResult()
    {
        ActionResult response = _testController.CheckoutSucceeded("testUrl");

        Assert.NotNull(response);
        Assert.IsType<RedirectResult>(response);
    }
    [Fact]
    public void CheckoutFailed_ShouldReturnRedirectResult()
    {
        ActionResult response = _testController.CheckoutFailed("testUrl");

        Assert.NotNull(response);
        Assert.IsType<RedirectResult>(response);
    }
}
