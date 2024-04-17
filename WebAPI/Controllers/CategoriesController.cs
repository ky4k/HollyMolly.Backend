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
    /// Allows to retrieve all category groups and their categories.
    /// </summary>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns a list of categories groups.</response>
    [Route("")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<CategoryGroupDto>>> GetAllCategories(CancellationToken cancellationToken)
    {
        return Ok(await categoryService.GetAllCategoryGroups(cancellationToken));
    }

    /// <summary>
    /// Allows to retrieve a category group and its categories.
    /// </summary>
    /// <param name="categoryGroupId">The id of the category group.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns a list of categories groups.</response>
    [Route("{categoryGroupId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryGroupDto?>> GetCategoryGroup(
        int categoryGroupId, CancellationToken cancellationToken)
    {
        return Ok(await categoryService.GetCategoryGroupByIdAsync(categoryGroupId, cancellationToken));
    }

    /// <summary>
    /// Allows administrators to create a new category group.
    /// </summary>
    /// <param name="categoryGroupDto">Category group to create.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category group was successfully created.</response>
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
    public async Task<ActionResult> CreateCategoryGroup(CategoryGroupCreateDto categoryGroupDto,
        CancellationToken cancellationToken)
    {
        OperationResult<CategoryGroupDto> result = await categoryService.CreateCategoryGroupAsync(categoryGroupDto,
            cancellationToken);
        return result.Succeeded && result.Payload != null
            ? CreatedAtAction(nameof(GetCategoryGroup), new { categoryGroupId = result.Payload.Id }, result.Payload)
            : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to update a category group.
    /// </summary>
    /// <param name="categoryGroupId">The id of the category group to update.</param>
    /// <param name="categoryGroupDto">The updated category group.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category group successfully updated.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{categoryGroupId}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateCategoryGroup(int categoryGroupId,
        CategoryGroupCreateDto categoryGroupDto, CancellationToken cancellationToken)
    {
        OperationResult result = await categoryService.UpdateCategoryGroupAsync(
            categoryGroupId, categoryGroupDto, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to update an image of the category group.
    /// </summary>
    /// <param name="categoryGroupId">The id of the category group to update image.</param>
    /// <param name="image">The updated image.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category group successfully updated.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{categoryGroupId}/image")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateCategoryGroupImage(int categoryGroupId,
        IFormFile image, CancellationToken cancellationToken)
    {
        string baseUrlPath = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        OperationResult result = await categoryService.UpdateCategoryGroupImageAsync(
            categoryGroupId, image, baseUrlPath, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to delete category group.
    /// </summary>
    /// <param name="categoryGroupId">The id of the category group to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category was successfully deleted.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{categoryGroupId}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteCategoryGroup(int categoryGroupId, CancellationToken cancellationToken)
    {
        OperationResult result = await categoryService.DeleteCategoryGroupAsync(
            categoryGroupId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows to retrieve a category.
    /// </summary>
    /// <param name="categoryGroupId">The id of the category group that contains the category.</param>
    /// <param name="categoryId">The id of the category to get.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns a list of categories groups.</response>
    [Route("{categoryGroupId}/{categoryId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<CategoryDto?>> GetCategory(int categoryGroupId,
        int categoryId, CancellationToken cancellationToken)
    {
        return Ok(await categoryService.GetCategoryByIdAsync(
            categoryGroupId, categoryId, cancellationToken));
    }

    /// <summary>
    /// Allows administrators to create a new category.
    /// </summary>
    /// <param name="categoryGroupId">The id of the category group to create a category in.</param>
    /// <param name="category">The category to create.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category was successfully created.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{categoryGroupId}")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateCategory(int categoryGroupId, CategoryCreateDto category,
        CancellationToken cancellationToken)
    {
        OperationResult<CategoryDto> result = await categoryService
            .CreateCategoryAsync(categoryGroupId, category, cancellationToken);
        return result.Succeeded && result.Payload != null
            ? CreatedAtAction(nameof(GetCategory),
                new { categoryGroupId = result.Payload.CategoryGroupId, categoryId = result.Payload.Id },
                result.Payload)
            : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to update a category.
    /// </summary>
    /// <param name="categoryGroupId">The id of the category group that contains the category
    /// to update.</param>
    /// <param name="categoryId">The ID of the category to update.</param>
    /// <param name="category">The updated category.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category was successfully updated.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{categoryGroupId}/{categoryId}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateCategory(int categoryGroupId, int categoryId,
        CategoryUpdateDto category, CancellationToken cancellationToken)
    {
        OperationResult result = await categoryService.UpdateCategoryAsync(categoryGroupId,
            categoryId, category, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to update an image of the category.
    /// </summary>
    /// <param name="categoryGroupId">The ID of the category group that contains the category
    /// to update image.</param>
    /// <param name="categoryId">The ID of the category to update.</param>
    /// <param name="image">The updated image.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category image was successfully updated.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{categoryGroupId}/{categoryId}/image")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> UpdateCategoryImage(int categoryGroupId,
        int categoryId, IFormFile image, CancellationToken cancellationToken)
    {
        string baseUrlPath = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        OperationResult result = await categoryService.UpdateCategoryImageAsync(
            categoryGroupId, categoryId, image, baseUrlPath, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to delete a category.
    /// </summary>
    /// <param name="categoryGroupId">The id of the category group that contains the category
    /// to delete.</param>
    /// <param name="categoryId">The ID of the category to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the category was successfully deleted.</response>
    /// <response code="400">Indicates that the request was invalid.</response>
    /// <response code="401">Indicates that the endpoint was called by an unauthenticated user.</response>
    /// <response code="403">Indicates that the user is not authorized to create a category.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{categoryGroupId}/{categoryId}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteCategory(int categoryGroupId, int categoryId, CancellationToken cancellationToken)
    {
        OperationResult result = await categoryService.DeleteCategoryAsync(
            categoryGroupId, categoryId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }
}
