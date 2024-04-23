using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Products;
using HM.DAL.Constants;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ProductsController(
    IProductService productService,
    IStatisticsService statisticsService
    ) : ControllerBase
{
    /// <summary>
    /// Allows to retrieve a list of products with optional filtering and sorting options.
    /// </summary>
    /// <param name="categoryGroupId">Optional. Filters products by category group. Has effect only
    /// if <paramref name="categoryId"/> is not specified.</param>
    /// <param name="categoryId">Optional. Filters products by category.</param>
    /// <param name="name">Optional. Filters products by name.</param>
    /// <param name="onlyNewCollection">Optional. Filters only products that have product instances from the new collection.</param>
    /// <param name="sortByPrice">Optional. If true, sorts products by price.</param>
    /// <param name="sortByRating">Optional. If true, sorts products by rating.
    ///     Is not applied if <paramref name="sortByPrice"/> is set to true</param>
    /// <param name="sortAsc">Optional. If true, sorts products in ascending order; otherwise, sorts in descending order.</param>
    /// <response code = "200" > Returns a list of products that match the specified criteria.</response>
    [Route("")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProductsByCategory(
        int? categoryGroupId, int? categoryId = null, string? name = null, bool onlyNewCollection = false,
        bool sortByPrice = false, bool sortByRating = false, bool sortAsc = true)
    {
        return Ok(await productService.GetProductsAsync(categoryGroupId, categoryId, name,
            onlyNewCollection, sortByPrice, sortByRating, sortAsc, Request.HttpContext.RequestAborted));
    }

    /// <summary>
    /// Allows to retrieve product information by its ID.
    /// </summary>
    /// <param name="productId">The ID of the product to retrieve.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the product information if found.</response>
    /// <response code="404">Indicates that the product with the specified ID was not found.</response>
    [Route("{productId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetProductById(int productId, CancellationToken cancellationToken)
    {
        ProductDto? productDto = await productService.GetProductByIdAsync(productId, cancellationToken);
        if (productDto == null)
        {
            return NotFound();
        }
        await statisticsService.AddToProductNumberViewsAsync(productId);
        return Ok(productDto);
    }

    /// <summary>
    /// Allows administrators and managers to create a new product.
    /// </summary>
    /// <param name="product">The product information to create.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="201">Indicates that the product was successfully created.</response>
    /// <response code="400">Indicates that the request to create the product was invalid.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}, {DefaultRoles.Manager}")]
    [Route("")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> CreateProduct(ProductCreateDto product, CancellationToken cancellationToken)
    {
        OperationResult<ProductDto> result = await productService.CreateProductAsync(product, cancellationToken);
        return result.Succeeded && result.Payload != null
            ? CreatedAtAction(nameof(GetProductById), new { productId = result.Payload.Id }, result.Payload)
            : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators and managers to update an existing product.
    /// </summary>
    /// <param name="productId">The ID of the product to update.</param>
    /// <param name="product">The updated product information.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the product was successfully updated.</response>
    /// <response code="400">Indicates that the request to update the product was invalid.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}, {DefaultRoles.Manager}")]
    [Route("{productId}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> UpdateProduct(int productId, ProductUpdateDto product, CancellationToken cancellationToken)
    {
        OperationResult<ProductDto> result = await productService.UpdateProductAsync(productId, product, cancellationToken);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators and managers to add new instance to the product.
    /// </summary>
    /// <param name="productId">Product to add instance to.</param>
    /// <param name="productInstance">Product instance to add.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the product instance was successfully added to the product.</response>
    /// <response code="400">Indicates that the request was invalid and returns the error message.</response>
    /// <response code="401">Indicates that the user is unauthenticated.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}, {DefaultRoles.Manager}")]
    [Route("{productId}/productInstances")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> AddProductInstance(int productId, ProductInstanceCreateDto productInstance,
        CancellationToken cancellationToken)
    {
        OperationResult<ProductInstanceDto> result = await productService.AddProductInstanceToProductAsync(
            productId, productInstance, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators and managers to update an existing product instance.
    /// </summary>
    /// <param name="productId">The ID of the product that contains instance.</param>
    /// <param name="productInstanceId">The ID of the product instance to update.</param>
    /// <param name="productInstanceDto">The updated product instance information.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the product instance was successfully updated.</response>
    /// <response code="400">Indicates that the request to update the product was invalid.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}, {DefaultRoles.Manager}")]
    [Route("{productId}/{productInstanceId}")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<ProductDto>> UpdateProductInstance(int productId, int productInstanceId,
        ProductInstanceCreateDto productInstanceDto, CancellationToken cancellationToken)
    {
        OperationResult<ProductInstanceDto> result = await productService.UpdateProductInstanceAsync(
            productId, productInstanceId, productInstanceDto, cancellationToken);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators to delete new instance from the product.
    /// </summary>
    /// <param name="productId">ID of the product that contains product instance.</param>
    /// <param name="productInstanceId">ID of the product instance to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the product instance was successfully deleted.</response>
    /// <response code="400">Indicates that the request was invalid and returns the error message.</response>
    /// <response code="401">Indicates that the user is unauthenticated.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}")]
    [Route("{productId}/{productInstanceId}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteProductInstance(int productId, int productInstanceId,
        CancellationToken cancellationToken)
    {
        OperationResult result = await productService.DeleteProductInstanceAsync(
            productId, productInstanceId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators and managers to upload the product images.
    /// </summary>
    /// <param name="productId">Id of the product that contains product instance to add image to.</param>
    /// <param name="productInstanceId">Id of the product instance that to add image to.</param>
    /// <param name="images">List of images to upload.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the message with the result of the operation.</response>
    /// <response code="400">Indicates that the request to upload the product images was invalid.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}, {DefaultRoles.Manager}")]
    [Route("{productId}/{productInstanceId}/images")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<string>> UploadProductImage(int productId,
        int productInstanceId, IFormFile[] images, CancellationToken cancellationToken)
    {
        string baseUrlPath = $"{Request.Scheme}://{Request.Host}{Request.PathBase}";
        OperationResult result = await productService.UploadProductImagesAsync(
            productId, productInstanceId, images, baseUrlPath, cancellationToken);
        return result.Succeeded ? Ok(result.Message) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators and managers to rearrange order of the images.
    /// </summary>
    /// <param name="productId">Id of the product that contains product instance.</param>
    /// <param name="productInstanceId">Id of the product instance to rearrange images.</param>
    /// <param name="imageRearrangesDto">List of image id's and its updated position.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that were rearranged.</response>
    /// <response code="400">Indicates that the request to delete the product image was invalid.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}, {DefaultRoles.Manager}")]
    [Route("{productId}/{productInstanceId}/images/")]
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> RearrangeProductImages(int productId, int productInstanceId,
        List<ProductImageRearrangeDto> imageRearrangesDto, CancellationToken cancellationToken)
    {
        OperationResult result = await productService.RearrangeProductImagesAsync(
            productId, productInstanceId, imageRearrangesDto, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators and managers to remove the product images.
    /// </summary>
    /// <param name="productId">Id of the product that contains product instance to remove image from.</param>
    /// <param name="productInstanceId">Id of the product instance that to remove image from.</param>
    /// <param name="imageId">Id of the image to remove.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the message with the result of the operation.</response>
    /// <response code="400">Indicates that the request to delete the product image was invalid.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}, {DefaultRoles.Manager}")]
    [Route("{productId}/{productInstanceId}/images/{imageId}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<string>> RemoveProductImage(int productId,
        int productInstanceId, int imageId, CancellationToken cancellationToken)
    {
        OperationResult result = await productService.DeleteProductImageAsync(
            productId, productInstanceId, imageId, cancellationToken);
        return result.Succeeded ? Ok(result.Message) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrator to delete product by its ID.
    /// </summary>
    /// <param name="productId">The ID of the product to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the product was successfully deleted.</response>
    /// <response code="400">Indicates that the request to delete the product was invalid.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("{productId}")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult> DeleteProduct(int productId, CancellationToken cancellationToken)
    {
        OperationResult result = await productService.DeleteProductAsync(productId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows administrators and managers to retrieve the feedback about all products or products in the specified category.
    /// </summary>
    /// <param name="categoryGroupId">Optional. Filters feedback for the products in the specified category group.
    /// Has effect only if <paramref name="categoryId"/> is not specified.</param>
    /// <param name="categoryId">Optional. Filters feedback for the products in the specified category.</param>
    /// <response code="200">Indicates that the feedback entries were successfully retrieved.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = $"{DefaultRoles.Administrator}, {DefaultRoles.Manager}")]
    [Route("feedback")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<IEnumerable<ProductFeedback>>> GetAllFeedback(
        int? categoryGroupId = null, int? categoryId = null)
    {
        return Ok(await productService.GetAllProductsFeedbackAsync(
            categoryGroupId, categoryId, Request.HttpContext.RequestAborted));
    }

    /// <summary>
    /// Allows to retrieve feedback for a specific product.
    /// </summary>
    /// <param name="productId">The ID of the product to retrieve feedback for.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Indicates that the feedback entries for the specified product were successfully retrieved.</response>
    /// <response code="400">Indicates that the request could not be processed due to invalid input.</response>
    [Route("feedback/{productId}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<ProductFeedbackDto>>> GetProductFeedback(
        int productId, CancellationToken cancellationToken)
    {
        OperationResult<IEnumerable<ProductFeedbackDto>> result = await productService
            .GetProductFeedbackAsync(productId, cancellationToken);
        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }

    /// <summary>
    /// Allows registered users to add feedback for a specific product.
    /// </summary>
    /// <param name="productId">The ID of the product to add feedback for.</param>
    /// <param name="feedback">The feedback information to be added.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the feedback was successfully added to the product.</response>
    /// <response code="400">Indicates that the request could not be processed due to invalid input.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    [Authorize]
    [Route("feedback/{productId}")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> AddFeedback(int productId, ProductFeedbackCreateDto feedback, CancellationToken cancellationToken)
    {
        string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        OperationResult result = await productService.AddFeedbackAsync(productId, userId, feedback, cancellationToken);
        if (result.Succeeded)
        {
            await statisticsService.AddToProductNumberFeedbacksAsync(productId);
            return NoContent();
        }
        else
        {
            return BadRequest(result.Message);
        }
    }

    /// <summary>
    /// Allows administrator to delete product feedback by its ID.
    /// </summary>
    /// <param name="productId">The ID of the product to delete.</param>
    /// <param name="feedbackId">The ID of the product to delete.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="204">Indicates that the product was successfully deleted.</response>
    /// <response code="400">Indicates that the request to delete the product was invalid.</response>
    /// <response code="401">Indicates that the request lacks valid authentication credentials for the target resource.</response>
    /// <response code="403">Indicates that the server understood the request but refuses to authorize it.</response>
    [Authorize(Roles = DefaultRoles.Administrator)]
    [Route("feedback/{productId}/{feedbackId}")]
    [HttpDelete]
    public async Task<ActionResult> DeleteFeedback(int productId, int feedbackId, CancellationToken cancellationToken)
    {
        OperationResult result = await productService
            .DeleteProductFeedbackAsync(productId, feedbackId, cancellationToken);
        return result.Succeeded ? NoContent() : BadRequest(result.Message);
    }
}
