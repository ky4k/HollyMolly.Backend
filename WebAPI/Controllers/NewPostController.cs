using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NewPostController(
    INewPostService newPostService
    ) : ControllerBase
{
    /// <summary>
    /// Allows to get list of cities from the New Post to fill the field City in customer address.
    /// </summary>
    /// <param name="name">Name of the city or its part to search.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the page that contains list of cities (up to 100).</response>
    /// <response code="400">Indicates that cities cannot be obtained and return the error message.</response>
    [Route("cities")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostCity>>> GetCities(
        string? name, CancellationToken cancellationToken)
    {
        OperationResult<IEnumerable<NewPostCity>> result = await newPostService
            .GetCitiesAsync(name, cancellationToken);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }
    /// <summary>
    /// Allows to get list of cities from the New Post warehouses in the specified city.
    /// </summary>
    /// <param name="cityKoatuu">Required. KOATUU of the city obtained from NewPost/cities endpoint.</param>
    /// <param name="warehouse">Optional. Name of the warehouse or its part to narrow search.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <param name="page">Page number with warehouses.</param>
    /// <response code="200">Returns the page that contains list of warehouses in the city (up to 100).</response>
    /// <response code="400">Indicates that warehouses cannot be obtained and return the error message.</response>
    [Route("warehouses")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostWarehouse>>> GetWarehouses(
        string? warehouse, string cityKoatuu, CancellationToken cancellationToken, int page = 1)
    {
        OperationResult<IEnumerable<NewPostWarehouse>> result = await newPostService
            .GetWarehousesAsync(warehouse, cityKoatuu, page, cancellationToken);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }
}
