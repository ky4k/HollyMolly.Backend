using HM.BLL.Interfaces;
using HM.BLL.Models.Categories;
using HM.BLL.Models.Common;
using HM.BLL.Models.Products;
using HM.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using WebAPI.UnitTests.TestHelpers;

namespace WebAPI.UnitTests.Controllers;

public class CategoriesControllerTests
{
    private readonly ICategoryService _categoryService;
    private readonly IProductService _productService;
    private readonly CategoriesController _categoriesController;
    public CategoriesControllerTests()
    {
        _categoryService = Substitute.For<ICategoryService>();
        _productService = Substitute.For<IProductService>();
        _categoriesController = new CategoriesController(_categoryService, _productService);
    }
    [Fact]
    public async Task GetAllCategories_ShouldReturnOkResult()
    {
        _categoryService.GetAllCategoryGroupsAsync(Arg.Any<CancellationToken>()).Returns([]);

        ActionResult<IEnumerable<CategoryGroupDto>> response = await _categoriesController
            .GetAllCategories(CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetCategoryGroup_ShouldReturnOkResult_WhenCategoryExist()
    {
        _categoryService.GetCategoryGroupByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CategoryGroupDto());

        ActionResult<CategoryGroupDto?> response = await _categoriesController
            .GetCategoryGroup(1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetCategoryGroup_ShouldReturnNotFoundResult_WhenCategoryDoesNotExist()
    {
        _categoryService.GetCategoryGroupByIdAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CategoryGroupDto?)null);

        ActionResult<CategoryGroupDto?> response = await _categoriesController
            .GetCategoryGroup(1, CancellationToken.None);
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    [Fact]
    public async Task GetCategoryGroupProducts_ShouldReturnOkResult()
    {
        _productService.GetProductsAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<ProductDto>> response = await _categoriesController
            .GetCategoryGroupProducts(1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task CreateCategoryGroup_ShouldReturnCreatedResult_WhenSucceeded()
    {
        CategoryGroupDto categoryGroupDto = new()
        {
            Id = 1,
            Name = "Category"
        };
        _categoryService.CreateCategoryGroupAsync(Arg.Any<CategoryGroupCreateDto>(), Arg.Any<CancellationToken>())
           .Returns(new OperationResult<CategoryGroupDto>(true, categoryGroupDto));

        ActionResult response = await _categoriesController
            .CreateCategoryGroup(new CategoryGroupCreateDto(), CancellationToken.None);
        var result = response as CreatedAtActionResult;

        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task CreateCategoryGroup_ShouldReturnBadRequest_WhenFailed()
    {
        _categoryService.CreateCategoryGroupAsync(Arg.Any<CategoryGroupCreateDto>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult<CategoryGroupDto>(false, "Failed"));

        ActionResult response = await _categoriesController
            .CreateCategoryGroup(new CategoryGroupCreateDto(), CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UpdateCategoryGroup_ShouldReturnNoContentResult_WhenSucceeded()
    {
        _categoryService.UpdateCategoryGroupAsync(Arg.Any<int>(), Arg.Any<CategoryGroupCreateDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));

        ActionResult response = await _categoriesController
            .UpdateCategoryGroup(1, new CategoryGroupCreateDto(), CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task UpdateCategoryGroup_ShouldReturnBadRequest_WhenFailed()
    {
        _categoryService.UpdateCategoryGroupAsync(Arg.Any<int>(), Arg.Any<CategoryGroupCreateDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _categoriesController
            .UpdateCategoryGroup(1, new CategoryGroupCreateDto(), CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UpdateCategoryGroupImage_ShouldReturnNoContentResult_WhenSucceeded()
    {
        ControllerHelper.MockHost(_categoriesController);
        _categoryService.UpdateCategoryGroupImageAsync(
            Arg.Any<int>(), Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _categoriesController
            .UpdateCategoryGroupImage(1, Substitute.For<IFormFile>(), CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task UpdateCategoryGroupImage_ShouldReturnBadRequest_WhenFailed()
    {
        ControllerHelper.MockHost(_categoriesController);
        _categoryService.UpdateCategoryGroupImageAsync(
            Arg.Any<int>(), Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _categoriesController
            .UpdateCategoryGroupImage(1, Substitute.For<IFormFile>(), CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task DeleteCategoryGroup_ShouldReturnNoContentResult_WhenSucceeded()
    {
        _categoryService.DeleteCategoryGroupAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _categoriesController.DeleteCategoryGroup(1, CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task DeleteCategoryGroup_ShouldReturnBadRequest_WhenFailed()
    {
        _categoryService.DeleteCategoryGroupAsync(Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _categoriesController.DeleteCategoryGroup(1, CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task GetCategory_ShouldReturnOkResult_WhenCategoryExist()
    {
        _categoryService.IsCategoryInCategoryGroupAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _categoryService.GetCategoryByIdAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CategoryDto());

        ActionResult<CategoryDto?> response = await _categoriesController
            .GetCategory(1, 1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetCategory_ShouldNotFound_WhenCategoryDoesNotExist()
    {
        _categoryService.IsCategoryInCategoryGroupAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _categoryService.GetCategoryByIdAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns((CategoryDto?)null);

        ActionResult<CategoryDto?> response = await _categoriesController
            .GetCategory(1, 1, CancellationToken.None);
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    [Fact]
    public async Task GetCategory_ShouldNotFound_WhenCategoryDoesNotExistInTheCategoryGroups()
    {
        _categoryService.IsCategoryInCategoryGroupAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _categoryService.GetCategoryByIdAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new CategoryDto());

        ActionResult<CategoryDto?> response = await _categoriesController
            .GetCategory(1, 1, CancellationToken.None);
        var result = response.Result as NotFoundResult;

        Assert.NotNull(result);
        Assert.Equal(404, result.StatusCode);
    }
    [Fact]
    public async Task GetCategoryProducts_ShouldReturnOkResult_WhenCategoryExist()
    {
        _categoryService.IsCategoryInCategoryGroupAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(true);
        _productService.GetProductsAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<ProductDto>> response = await _categoriesController
            .GetCategoryProducts(1, 1, CancellationToken.None);
        var result = response.Result as OkObjectResult;

        Assert.NotNull(result);
        Assert.Equal(200, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task GetCategoryProducts_ShouldReturnBadRequest_WhenCategoryDoesNotExistInTheCategoryGroup()
    {
        _categoryService.IsCategoryInCategoryGroupAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(false);
        _productService.GetProductsAsync(Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<string?>(),
            Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
            .Returns([]);

        ActionResult<IEnumerable<ProductDto>> response = await _categoriesController
            .GetCategoryProducts(1, 1, CancellationToken.None);
        var result = response.Result as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task CreateCategory_ShouldReturnCreatedResult_WhenSucceeded()
    {
        CategoryDto categoryDto = new()
        {
            Id = 1,
            Name = "Category"
        };
        _categoryService.CreateCategoryAsync(Arg.Any<int>(), Arg.Any<CategoryCreateDto>(), Arg.Any<CancellationToken>())
           .Returns(new OperationResult<CategoryDto>(true, categoryDto));

        ActionResult response = await _categoriesController
            .CreateCategory(1, new CategoryCreateDto(), CancellationToken.None);
        var result = response as CreatedAtActionResult;

        Assert.NotNull(result);
        Assert.Equal(201, result.StatusCode);
        Assert.NotNull(result.Value);
    }
    [Fact]
    public async Task CreateCategory_ShouldReturnBadRequest_WhenFailed()
    {
        _categoryService.CreateCategoryAsync(Arg.Any<int>(), Arg.Any<CategoryCreateDto>(), Arg.Any<CancellationToken>())
           .Returns(new OperationResult<CategoryDto>(false, "Failed!"));

        ActionResult response = await _categoriesController
            .CreateCategory(1, new CategoryCreateDto(), CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UpdateCategory_ShouldReturnNoContent_WhenSucceeded()
    {
        _categoryService.UpdateCategoryAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CategoryUpdateDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(true));

        ActionResult response = await _categoriesController.UpdateCategory(
            1, 1, new CategoryUpdateDto(), CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task UpdateCategory_ShouldReturnBadRequest_WhenFailed()
    {
        _categoryService.UpdateCategoryAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CategoryUpdateDto>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _categoriesController.UpdateCategory(
            1, 1, new CategoryUpdateDto(), CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task UpdateCategoryImage_ShouldReturnNoContent_WhenSucceeded()
    {
        ControllerHelper.MockHost(_categoriesController);
        _categoryService.UpdateCategoryImageAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _categoriesController
            .UpdateCategoryImage(1, 1, Substitute.For<IFormFile>(), CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task UpdateCategoryImage_ShouldReturnBadRequest_WhenFailed()
    {
        ControllerHelper.MockHost(_categoriesController);
        _categoryService.UpdateCategoryImageAsync(Arg.Any<int>(), Arg.Any<int>(),
            Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _categoriesController
            .UpdateCategoryImage(1, 1, Substitute.For<IFormFile>(), CancellationToken.None);

        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
    [Fact]
    public async Task DeleteCategory_ShouldReturnNoContent_WhenSucceeded()
    {
        _categoryService.DeleteCategoryAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(true));

        ActionResult response = await _categoriesController.DeleteCategory(1, 1, CancellationToken.None);
        var result = response as NoContentResult;

        Assert.NotNull(result);
        Assert.Equal(204, result.StatusCode);
    }
    [Fact]
    public async Task DeleteCategory_ShouldReturnBadRequest_WhenFailed()
    {
        _categoryService.DeleteCategoryAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
            .Returns(new OperationResult(false, "Failed!"));

        ActionResult response = await _categoriesController.DeleteCategory(1, 1, CancellationToken.None);
        var result = response as BadRequestObjectResult;

        Assert.NotNull(result);
        Assert.Equal(400, result.StatusCode);
    }
}
