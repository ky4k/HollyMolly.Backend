using HM.BLL.Interfaces;
using HM.BLL.Models.Categories;
using HM.BLL.Models.Common;
using HM.BLL.Models.Products;
using HM.DAL.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CategoriesController(
    ICategoryService categoryService,
    IProductService productService
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
    /// <response code="404">Indicates that there are no category group with such an id.</response>
    [Route("{categoryGroupId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryGroupDto?>> GetCategoryGroup(
        int categoryGroupId, CancellationToken cancellationToken)
    {
        CategoryGroupDto? categoryGroup = await categoryService.GetCategoryGroupByIdAsync(categoryGroupId, cancellationToken);
        return categoryGroup == null ? NotFound() : Ok(categoryGroup);
    }

    /// <summary>
    /// Allows to retrieve the all products of the specific category group.
    /// </summary>
    /// <param name="categoryGroupId">Id of the category group to retrieve products.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns a list of products in the category group.</response>
    [Route("{categoryGroupId}/products")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetCategoryGroupProducts(
        int categoryGroupId, CancellationToken cancellationToken)
    {
        return Ok(await productService.GetProductsAsync(categoryGroupId, null,
            null, false, false, false, true, cancellationToken));
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
    /// <response code="404">Indicates that there are no category with such an id in the specified category group.</response>
    [Route("{categoryGroupId}/{categoryId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CategoryDto?>> GetCategory(int categoryGroupId,
        int categoryId, CancellationToken cancellationToken)
    {
        if (!await categoryService.IsCategoryInCategoryGroupAsync(categoryGroupId, categoryId, cancellationToken))
        {
            return NotFound();
        }
        CategoryDto? category = await categoryService.GetCategoryByIdAsync(categoryGroupId, categoryId, cancellationToken);
        return category == null ? NotFound() : Ok(category);
    }

    /// <summary>
    /// Allows to retrieve the all products of the specific category group.
    /// </summary>
    /// <param name="categoryGroupId">Id of the category group to retrieve products.</param>
    /// <param name="categoryId">Id of the category to retrieve products.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns a list of products in the product in the specific category.</response>
    [Route("{categoryGroupId}/{categoryId}/products")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetCategoryProducts(int categoryGroupId,
        int categoryId, CancellationToken cancellationToken)
    {
        if (!await categoryService.IsCategoryInCategoryGroupAsync(categoryGroupId, categoryId, cancellationToken))
        {
            return BadRequest($"Category group with id {categoryGroupId} does not contain category with id {categoryId}");
        }
        return Ok(await productService.GetProductsAsync(categoryGroupId, categoryId,
            null, false, false, false, true, cancellationToken));
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
