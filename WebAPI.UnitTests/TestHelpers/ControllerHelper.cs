using HM.BLL.Models.Users;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;

namespace WebAPI.UnitTests.TestHelpers;

public static class ControllerHelper
{
    public static void MockUserIdentity(UserDto userDto, ControllerBase controller)
    {
        var userMock = Substitute.For<ClaimsPrincipal>();
        if (userDto.Id != null)
        {
            userMock.FindFirst(ClaimTypes.NameIdentifier)
                .Returns(new Claim(ClaimTypes.NameIdentifier, userDto.Id));
        }
        if (userDto.Email != null)
        {
            userMock.FindFirst(ClaimTypes.Name)
                .Returns(new Claim(ClaimTypes.Name, userDto.Email!));
            userMock.FindFirst(ClaimTypes.Email)
                .Returns(new Claim(ClaimTypes.Email, userDto.Email!));
        }
        foreach (var role in userDto.Roles)
        {
            userMock.FindFirst(ClaimTypes.Role)
                .Returns(new Claim(ClaimTypes.Role, role));
            userMock.IsInRole(Arg.Is(role)).Returns(true);
        }

        var httpContextMock = Substitute.For<HttpContext>();
        httpContextMock.User.Returns(userMock);

        var controllerContext = new ControllerContext
        {
            HttpContext = httpContextMock,
            RouteData = new() { },
            ActionDescriptor = new()
        };

        controller.ControllerContext = new ControllerContext(controllerContext);
    }
}
