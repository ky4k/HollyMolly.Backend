using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Products;
using HM.BLL.Models.Users;
using HM.DAL.Constants;
using HM.DAL.Entities;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class ProductControllerTests
{
    private readonly IProductService _productService;
    private readonly IStatisticsService _statisticsService;
    private readonly ProductsController _productsController;
    public ProductControllerTests()
    {
        _productService = Substitute.For<IProductService>();
        _statisticsService = Substitute.For<IStatisticsService>();
        _productsController = new ProductsController(_productService, _statisticsService);
    }
    [Fact]
    public async Task GetProductsByCategory_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_productsController);
        _productService.GetProductsAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([new ProductDto()]);

        ActionResult<IEnumerable<ProductDto>> response = await _productsController.GetProductsByCategory(1);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetRecommendedProducts_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_productsController);
        _productService.GetRecommendedProductsAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns([]);

        ActionResult<IEnumerable<ProductDto>> response = await _productsController.GetRecommendedProducts();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetProductById_ShouldReturnOkResult_WhenProductExists()
    {
        _productService.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ProductDto());

        ActionResult<ProductDto> response = await _productsController.GetProductById(1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetProductById_ShouldReturnNotFound_WhenProductDoesNotExist()
    {
        _productService.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((ProductDto?)null);

        ActionResult<ProductDto> response = await _productsController.GetProductById(1, CancellationToken.None);
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    [Fact]
    public async Task GetProductById_ShouldModifyProductStatistic()
    {
        _productService.GetProductByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new ProductDto());

        ActionResult<ProductDto> response = await _productsController.GetProductById(1, CancellationToken.None);

        Assert.NotNull(response);
        await _statisticsService.Received().AddToProductNumberViewsAsync(Arg.Any<int>());
    }
    [Fact]
    public async Task CreateProduct_ShouldReturnCreatedResult_WhenSucceeded()
    {
        _productService.CreateProductAsync(Arg.Any<ProductCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductDto>(true, new ProductDto()));

        ActionResult<ProductDto> response = await _productsController
            .CreateProduct(new ProductCreateDto(), CancellationToken.None);
        var result = response.Result as CreatedAtActionResult;

        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task CreateProduct_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.CreateProductAsync(Arg.Any<ProductCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductDto>(false, "Failed!"));

        ActionResult<ProductDto> response = await _productsController
            .CreateProduct(new ProductCreateDto(), CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UpdateProduct_ShouldReturnOkResult_WhenSucceeded()
    {
        _productService.UpdateProductAsync(Arg.Any<int>(), Arg.Any<ProductUpdateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductDto>(true, new ProductDto()));

        ActionResult<ProductDto> response = await _productsController
            .UpdateProduct(1, new ProductUpdateDto(), CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task UpdateProduct_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.UpdateProductAsync(Arg.Any<int>(), Arg.Any<ProductUpdateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductDto>(false, "Failed!"));

        ActionResult<ProductDto> response = await _productsController
            .UpdateProduct(1, new ProductUpdateDto(), CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task AddProductInstance_ShouldReturnOkResult_WhenSucceeded()
    {
        _productService.AddProductInstanceToProductAsync(
            Arg.Any<int>(), Arg.Any<ProductInstanceCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductInstanceDto>(true, new ProductInstanceDto()));

        ActionResult<ProductInstanceDto> response = await _productsController
            .AddProductInstance(1, new ProductInstanceCreateDto(), CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task AddProductInstance_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.AddProductInstanceToProductAsync(
            Arg.Any<int>(), Arg.Any<ProductInstanceCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductInstanceDto>(false, "Failed!"));

        ActionResult<ProductInstanceDto> response = await _productsController
            .AddProductInstance(1, new ProductInstanceCreateDto(), CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UpdateProductInstance_ShouldReturnOkResult_WhenSucceeded()
    {
        _productService.UpdateProductInstanceAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ProductInstanceCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductInstanceDto>(true, new ProductInstanceDto()));

        ActionResult<ProductInstanceDto> response = await _productsController
            .UpdateProductInstance(1, 1, new ProductInstanceCreateDto(), CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task UpdateProductInstance_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.UpdateProductInstanceAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<ProductInstanceCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductInstanceDto>(false, "Failed!"));

        ActionResult<ProductInstanceDto> response = await _productsController
            .UpdateProductInstance(1, 1, new ProductInstanceCreateDto(), CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task DeleteProductInstance_ShouldReturnNoContent_WhenSucceeded()
    {
        _productService.DeleteProductInstanceAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _productsController.DeleteProductInstance(1, 1, CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task DeleteProductInstance_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.DeleteProductInstanceAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false));

        ActionResult response = await _productsController.DeleteProductInstance(1, 1, CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UploadProductImage_ShouldReturnOkResult_WhenSucceeded()
    {
        ControllerHelper.MockHost(_productsController);
        _productService.UploadProductImagesAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductInstanceDto>(true, "Succeeded!", new ProductInstanceDto()));

        ActionResult<string> response = await _productsController
            .UploadProductImage(1, 1, [], CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task UploadProductImage_ShouldReturnBadRequest_WhenFailed()
    {
        ControllerHelper.MockHost(_productsController);
        _productService.UploadProductImagesAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<ProductInstanceDto>(false, "Failed!"));

        ActionResult<string> response = await _productsController
            .UploadProductImage(1, 1, [], CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task RearrangeProductImages_ShouldReturnNoContent_WhenSucceeded()
    {
        _productService.RearrangeProductImagesAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<List<ProductImageRearrangeDto>>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _productsController
            .RearrangeProductImages(1, 1, [], CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task RearrangeProductImages_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.RearrangeProductImagesAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<List<ProductImageRearrangeDto>>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false));

        ActionResult response = await _productsController
            .RearrangeProductImages(1, 1, [], CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task RemoveProductImage_ShouldReturnOkResult_WhenSucceeded()
    {
        _productService.DeleteProductImageAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true, "Succeeded!"));

        ActionResult<string> response = await _productsController
            .RemoveProductImage(1, 1, 1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task RemoveProductImage_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.DeleteProductImageAsync(
            Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult<string> response = await _productsController.RemoveProductImage(1, 1, 1, CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task DeleteProduct_ShouldReturnNoContent_WhenSucceeded()
    {
        _productService.DeleteProductAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _productsController.DeleteProduct(1, CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task DeleteProduct_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.DeleteProductAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _productsController.DeleteProduct(1, CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task GetAllFeedback_ShouldReturnOkResult()
    {
        ControllerHelper.MockHost(_productsController);
        _productService.GetAllProductsFeedbackAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<ProductFeedback>> response = await _productsController.GetAllFeedback();
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetProductFeedback_ShouldReturnOkResult_WhenExist()
    {
        _productService.GetProductFeedbackAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<IEnumerable<ProductFeedbackDto>>(true, "", []));

        ActionResult<IEnumerable<ProductFeedbackDto>> response = await _productsController
            .GetProductFeedback(1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetProductFeedback_ShouldReturnBadRequest_WhenNotExist()
    {
        _productService.GetProductFeedbackAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<IEnumerable<ProductFeedbackDto>>(false, "Failed!"));

        ActionResult<IEnumerable<ProductFeedbackDto>> response = await _productsController
            .GetProductFeedback(1, CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task AddFeedback_ShouldReturnNoContent_WhenSucceeded()
    {
        UserDto? userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _productsController);
        _productService.AddFeedbackAsync(Arg.Any<int>(), Arg.Any<string?>(),
            Arg.Any<ProductFeedbackCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _productsController
            .AddFeedback(1, new ProductFeedbackCreateDto(), CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task AddFeedback_ShouldModifyProductStatistic()
    {
        UserDto? userDto = new()
        {
            Id = null!,
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _productsController);
        _productService.AddFeedbackAsync(Arg.Any<int>(), Arg.Any<string?>(),
            Arg.Any<ProductFeedbackCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _productsController
            .AddFeedback(1, new ProductFeedbackCreateDto(), CancellationToken.None);

        Assert.NotNull(response);
        await _statisticsService.Received().AddToProductNumberFeedbacksAsync(Arg.Any<int>());
    }
    [Fact]
    public async Task AddFeedback_ShouldReturnBadRequest_WhenFailed()
    {
        UserDto? userDto = new()
        {
            Id = "1",
            Email = "user1@example.com",
            Roles = [DefaultRoles.User]
        };
        ControllerHelper.MockUserIdentity(userDto, _productsController);
        _productService.AddFeedbackAsync(Arg.Any<int>(), Arg.Any<string?>(),
            Arg.Any<ProductFeedbackCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _productsController
            .AddFeedback(1, new ProductFeedbackCreateDto(), CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task DeleteFeedback_ShouldReturnNoContent_WhenSucceeded()
    {
        _productService.DeleteProductFeedbackAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _productsController.DeleteFeedback(1, 1, CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task DeleteFeedback_ShouldReturnBadRequest_WhenFailed()
    {
        _productService.DeleteProductFeedbackAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _productsController.DeleteFeedback(1, 1, CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
}
