using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController(
    ICategoryService categoryService
    ) : ControllerBase
{
    /// <summary>
    /// Retrieves all categories.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns a list of category names.</response>
    [Route("")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<string>>> GetAllCategories(CancellationToken cancellationToken)
    {
        return Ok(await categoryService.GetAllCategories(cancellationToken));
    }

    /// <summary>
    /// Allows administrators to create a new category.
    /// </summary>
    /// <param name="category">The category to create.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category was successfully created.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateCategory(CategoryCreateDto category, CancellationToken cancellationToken)
    {
        OperationResult result = await categoryService.CreateCategoryAsync(category, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to update a category.
    /// </summary>
    /// <param name="category">The category to update.</param>
    /// <param name="updatedCategory">The updated fields of the category.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category was successfully updated.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{category}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateCategory(string category, CategoryCreateDto updatedCategory,
        CancellationToken cancellationToken)
    {
        OperationResult result = await categoryService.UpdateCategoryAsync(category, updatedCategory, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to delete a category.
    /// </summary>
    /// <param name="category">The category to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category was successfully deleted.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{category}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteCategory(string category, CancellationToken cancellationToken)
    {
        OperationResult result = await categoryService.DeleteCategoryAsync(category, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }
}
