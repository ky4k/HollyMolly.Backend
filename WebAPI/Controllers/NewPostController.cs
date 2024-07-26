using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NewPostController(
    INewPostService newPostService
    ) : ControllerBase
{
    /// <summary>
    /// Allows to get a list of cities from the New Post.
    /// </summary>
    /// <param name="findByString">Optional. Part of the city name to search for.</param>
    /// <param name="ref">Optional. Reference of the city.</param>
    /// <param name="page">Optional. Page number of cities. Defaults to 1.</param>
    /// <param name="limit">Optional. Number of items per page. Defaults to 100.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the page that contains a list of cities.</response>
    /// <response code="400">Indicates that cities cannot be obtained and returns the error message.</response>
    [Route("cities")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostCities>>> GetCities(
        string? findByString = null,
        string? @ref = null,
        int page = 1,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        var result = await newPostService.GetCitiesAsync(
            FindByString: findByString,
            Ref: @ref,
            Page: page.ToString(),
            Limit: limit.ToString(),
            cancellationToken: cancellationToken);

        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows to get a list of warehouses from the New Post in the specified city.
    /// </summary>
    /// <param name="cityName">Optional. Name of the city to search within.</param>
    /// <param name="warehouseId">Optional. ID of the specific warehouse to search for.</param>
    /// <param name="findByString">Optional. Part of the warehouse name to search for.</param>
    /// <param name="cityRef">Optional. Reference of the city obtained from NewPost/cities endpoint.</param>
    /// <param name="page">Optional. Page number of warehouses. Defaults to 1.</param>
    /// <param name="limit">Optional. Number of items per page. Defaults to 100.</param>
    /// <param name="language">Optional. Language for the response.</param>
    /// <param name="typeOfWarehouseRef">Optional. Type of the warehouse reference.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the page that contains a list of warehouses in the city (up to 100).</response>
    /// <response code="400">Indicates that warehouses cannot be obtained and returns the error message.</response>
    [Route("warehouses")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostWarehouse>>> GetWarehouses(
        string? cityName = null,
        string? warehouseId = null,
        string? findByString = null,
        string? cityRef = null,
        int page = 1,
        int limit = 100,
        string? language = null,
        string? typeOfWarehouseRef = null,
        CancellationToken cancellationToken = default)
    {
        var result = await newPostService.GetWarehousesAync(
            CityName: cityName,
            WarehouseId: warehouseId,
            FindByString: findByString,
            CityRef: cityRef,
            Page: page.ToString(),
            Limit: limit.ToString(),
            Language: language,
            TypeOfWarehouseRef: typeOfWarehouseRef,
            cancellationToken: cancellationToken);

        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }
}
