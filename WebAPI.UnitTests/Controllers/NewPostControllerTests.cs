using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;

namespace WebAPI.UnitTests.Controllers;

public class NewPostControllerTests
{
    private readonly INewPostCityesService _newPostService;
    private readonly NewPostController _newPostController;
    public NewPostControllerTests()
    {
        _newPostService = Substitute.For<INewPostCityesService>();
        _newPostController = new NewPostController(_newPostService);
    }
    [Fact]
    public async Task GetCities_ShouldReturnOkResult_WhenSucceeded()
    {
        _newPostService.GetCitiesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<IEnumerable<NewPostCity>>(true, Array.Empty<NewPostCity>()));

        ActionResult<IEnumerable<NewPostCity>> response = await _newPostController
            .GetCities("1", CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetCities_ShouldReturnBadRequest_WhenFailed()
    {
        _newPostService.GetCitiesAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<IEnumerable<NewPostCity>>(false, "Failed!"));

        ActionResult<IEnumerable<NewPostCity>> response = await _newPostController
            .GetCities("1", CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task GetWarehouses_ShouldReturnOkResult_WhenSucceeded()
    {
        _newPostService.GetWarehousesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<IEnumerable<NewPostWarehouse>>(true, Array.Empty<NewPostWarehouse>()));

        ActionResult<IEnumerable<NewPostWarehouse>> response = await _newPostController
            .GetWarehouses("1", "1", CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetWarehouses_ShouldReturnBadRequest_WhenFailed()
    {
        _newPostService.GetWarehousesAsync(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Failed!"));

        ActionResult<IEnumerable<NewPostWarehouse>> response = await _newPostController
            .GetWarehouses("1", "1", CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
}
