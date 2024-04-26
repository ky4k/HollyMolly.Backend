using HM.BLL.Interfaces;
using HM.BLL.Models.Categories;
using HM.BLL.Models.Common;
using HM.BLL.Services;
using HM.BLL.UnitTests.Helpers;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace HM.BLL.UnitTests.Services;

public class CategoryServiceTests
{
    private readonly HmDbContext _context;
    private readonly IImageService _imageService;
    private readonly ILogger<CategoryService> _logger;
    private readonly CategoryService _categoryService;
    public CategoryServiceTests()
    {
        _imageService = Substitute.For<IImageService>();
        _logger = Substitute.For<ILogger<CategoryService>>();
        _context = ServiceHelper.GetTestDbContext();
        _categoryService = new CategoryService(_context, _imageService, _logger);
    }
    [Fact]
    public async Task GetAllCategoryGroupsAsync_ShouldReturnAllCategoryGroups()
    {
        await SeedDbContextAsync();

        IEnumerable<CategoryGroupDto> categoryGroups = await _categoryService
            .GetAllCategoryGroupsAsync(CancellationToken.None);

        Assert.Equal(3, categoryGroups.Count());
        Assert.Equal(5, categoryGroups.SelectMany(cg => cg.Categories).Count());
    }
    [Fact]
    public async Task GetAllCategoryGroupsAsync_ShouldReturnEmptyList_WhenNoCategoryGroupExist()
    {
        IEnumerable<CategoryGroupDto> categoryGroups = await _categoryService
            .GetAllCategoryGroupsAsync(CancellationToken.None);

        Assert.Empty(categoryGroups);
    }
    [Fact]
    public async Task GetCategoryGroupByIdAsync_ShouldReturnCorrectCategoryGroup()
    {
        await SeedDbContextAsync();

        CategoryGroupDto? categoryGroup = await _categoryService
            .GetCategoryGroupByIdAsync(1, CancellationToken.None);

        Assert.NotNull(categoryGroup);
        Assert.Equal("Category group 1", categoryGroup.Name);
    }
    [Fact]
    public async Task GetCategoryGroupByIdAsync_ShouldReturnNull_WhenCategoryGroupDoesNotExist()
    {
        await SeedDbContextAsync();

        CategoryGroupDto? categoryGroup = await _categoryService
            .GetCategoryGroupByIdAsync(999, CancellationToken.None);

        Assert.Null(categoryGroup);
    }
    [Fact]
    public async Task CreateCategoryGroupAsync_ShouldCreateNewCategoryGroup()
    {
        await SeedDbContextAsync();
        CategoryGroupCreateDto categoryGroupDto = new()
        {
            Name = "New category group.",
            Position = 1
        };

        OperationResult<CategoryGroupDto> result = await _categoryService
            .CreateCategoryGroupAsync(categoryGroupDto, CancellationToken.None);
        CategoryGroup? categoryGroup = await _context.CategoryGroups
            .FirstOrDefaultAsync(cg => result.Payload != null && cg.Id == result.Payload.Id);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.NotNull(categoryGroup);
    }
    [Fact]
    public async Task CreateCategoryGroupAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new CategoryService(dbContextMock, _imageService, _logger);
        CategoryGroupCreateDto categoryGroupDto = new()
        {
            Name = "New category group.",
            Position = 1
        };

        OperationResult<CategoryGroupDto> result = await service
            .CreateCategoryGroupAsync(categoryGroupDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryGroupAsync_ShouldUpdateCategoryGroup()
    {
        await SeedDbContextAsync();
        CategoryGroupCreateDto categoryGroupDto = new()
        {
            Name = "Updated name",
            Position = 5
        };

        OperationResult result = await _categoryService
            .UpdateCategoryGroupAsync(1, categoryGroupDto, CancellationToken.None);
        CategoryGroup? updatedCategory = await _context.CategoryGroups
            .FirstOrDefaultAsync(cg => cg.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(updatedCategory);
        Assert.Equal(categoryGroupDto.Name, updatedCategory.Name);
        Assert.Equal(categoryGroupDto.Position, updatedCategory.Position);
    }
    [Fact]
    public async Task UpdateCategoryGroupAsync_ShouldReturnFalseResult_WhenCategoryGroupDoesNotExist()
    {
        await SeedDbContextAsync();
        CategoryGroupCreateDto categoryGroupDto = new()
        {
            Name = "Updated name",
            Position = 5
        };

        OperationResult result = await _categoryService
            .UpdateCategoryGroupAsync(999, categoryGroupDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryGroupAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new CategoryService(dbContextMock, _imageService, _logger);
        CategoryGroupCreateDto categoryGroupDto = new()
        {
            Name = "Updated name",
            Position = 5
        };

        OperationResult result = await service
            .UpdateCategoryGroupAsync(1, categoryGroupDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryGroupImageAsync_ShouldAddImageToCategoryGroup()
    {
        await SeedDbContextAsync();
        ImageDto image = Image;
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryGroupImageAsync(
            1, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);
        CategoryGroup? categoryGroup = await _context.CategoryGroups
            .FirstOrDefaultAsync(cg => cg.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(categoryGroup);
        Assert.Equal(image.Link, categoryGroup.ImageLink);
    }
    [Fact]
    public async Task UpdateCategoryGroupImageAsync_ShouldRemoveOldImage()
    {
        await SeedDbContextAsync();
        ImageDto image = Image;
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryGroupImageAsync(
            2, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        _imageService.Received().DeleteImage(Arg.Any<string>());
    }
    [Fact]
    public async Task UpdateCategoryGroupImageAsync_ShouldReturnFalseResult_WhenCategoryGroupDoesNotExist()
    {
        await SeedDbContextAsync();
        ImageDto image = Image;
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryGroupImageAsync(
            999, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryGroupImageAsync_ShouldReturnFalseResult_WhenImageWasNotUploaded()
    {
        await SeedDbContextAsync();
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(false));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryGroupImageAsync(
            1, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryGroupImageAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new CategoryService(dbContextMock, _imageService, _logger);
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, Image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await service.UpdateCategoryGroupImageAsync(
            1, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteCategoryGroupAsync_ShouldDeleteCategoryGroup()
    {
        await SeedDbContextAsync();
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.DeleteCategoryGroupAsync(3, CancellationToken.None);
        CategoryGroup? categoryGroup = await _context.CategoryGroups.FirstOrDefaultAsync(cg => cg.Id == 3);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Null(categoryGroup);
        _imageService.Received().DeleteImage(Arg.Any<string>());
    }
    [Fact]
    public async Task DeleteCategoryGroupAsync_ShouldReturnFalseResult_WhenCategoryGroupDoesNotExist()
    {
        await SeedDbContextAsync();
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.DeleteCategoryGroupAsync(999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteCategoryGroupAsync_ShouldReturnFalseResult_WhenCategoryGroupIsNotEmpty()
    {
        await SeedDbContextAsync();
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.DeleteCategoryGroupAsync(1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteCategoryGroupAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new CategoryService(dbContextMock, _imageService, _logger);
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await service.DeleteCategoryGroupAsync(3, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnCorrectCategory()
    {
        await SeedDbContextAsync();

        CategoryDto? category = await _categoryService
            .GetCategoryByIdAsync(1, 1, CancellationToken.None);

        Assert.NotNull(category);
        Assert.Equal("Category 1", category.Name);
    }
    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenCategoryDoesNotExist()
    {
        await SeedDbContextAsync();

        CategoryDto? category = await _categoryService
            .GetCategoryByIdAsync(1, 999, CancellationToken.None);

        Assert.Null(category);
    }
    [Fact]
    public async Task GetCategoryByIdAsync_ShouldReturnNull_WhenCategoryDoesNotBelongToTheCategoryGroup()
    {
        await SeedDbContextAsync();

        CategoryDto? category = await _categoryService
            .GetCategoryByIdAsync(1, 3, CancellationToken.None);

        Assert.Null(category);
    }
    [Fact]
    public async Task IsCategoryInCategoryGroupAsync_ShouldReturnTrue_WhenCategoryBelongsToTheCategoryGroup()
    {
        await SeedDbContextAsync();

        bool isInCategoryGroup = await _categoryService
            .IsCategoryInCategoryGroupAsync(1, 2, CancellationToken.None);

        Assert.True(isInCategoryGroup);
    }
    [Fact]
    public async Task IsCategoryInCategoryGroupAsync_ShouldReturnFalse_WhenCategoryDoesNotBelongToTheCategoryGroup()
    {
        await SeedDbContextAsync();

        bool isInCategoryGroup = await _categoryService
            .IsCategoryInCategoryGroupAsync(1, 4, CancellationToken.None);

        Assert.False(isInCategoryGroup);
    }
    [Fact]
    public async Task IsCategoryInCategoryGroupAsync_ShouldReturnFalse_WhenCategoryGroupDoesNotExist()
    {
        await SeedDbContextAsync();

        bool isInCategoryGroup = await _categoryService
            .IsCategoryInCategoryGroupAsync(999, 1, CancellationToken.None);

        Assert.False(isInCategoryGroup);
    }
    [Fact]
    public async Task IsCategoryInCategoryGroupAsync_ShouldReturnFalse_WhenCategoryDoesNotExist()
    {
        await SeedDbContextAsync();

        bool isInCategoryGroup = await _categoryService
            .IsCategoryInCategoryGroupAsync(1, 999, CancellationToken.None);

        Assert.False(isInCategoryGroup);
    }
    [Fact]
    public async Task CreateCategoryAsync_ShouldCreateNewCategory()
    {
        await SeedDbContextAsync();
        CategoryCreateDto categoryDto = new()
        {
            CategoryName = "New category group.",
            Position = 1
        };

        OperationResult<CategoryDto> result = await _categoryService
            .CreateCategoryAsync(1, categoryDto, CancellationToken.None);
        Category? category = await _context.Categories
            .FirstOrDefaultAsync(c => result.Payload != null && c.Id == result.Payload.Id);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.NotNull(category);
        Assert.Equal(1, category.CategoryGroupId);
    }
    [Fact]
    public async Task CreateCategoryAsync_ShouldReturnFalseResult_WhenCategoryGroupDoesNotExist()
    {
        await SeedDbContextAsync();
        CategoryCreateDto categoryDto = new()
        {
            CategoryName = "New category group.",
            Position = 1
        };

        OperationResult<CategoryDto> result = await _categoryService
            .CreateCategoryAsync(999, categoryDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task CreateCategoryAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new CategoryService(dbContextMock, _imageService, _logger);
        CategoryCreateDto categoryDto = new()
        {
            CategoryName = "New category group.",
            Position = 1
        };

        OperationResult<CategoryDto> result = await service
            .CreateCategoryAsync(1, categoryDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryAsync_ShouldUpdateCategory()
    {
        await SeedDbContextAsync();
        CategoryUpdateDto categoryDto = new()
        {
            CategoryName = "New category group.",
            CategoryGroupId = 2,
            Position = 5
        };

        OperationResult result = await _categoryService
            .UpdateCategoryAsync(1, 1, categoryDto, CancellationToken.None);
        Category? category = await _context.Categories.FirstOrDefaultAsync(c => c.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(category);
        Assert.Multiple(
            () => Assert.Equal(categoryDto.CategoryName, category.Name),
            () => Assert.Equal(categoryDto.CategoryGroupId, category.CategoryGroupId),
            () => Assert.Equal(categoryDto.Position, category.Position)
        );
    }
    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnFalseResult_WhenCategoryGroupDoesNotExist()
    {
        await SeedDbContextAsync();
        CategoryUpdateDto categoryDto = new()
        {
            CategoryName = "New category group.",
            CategoryGroupId = 2,
            Position = 5
        };

        OperationResult result = await _categoryService
            .UpdateCategoryAsync(999, 1, categoryDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnFalseResult_WhenCategoryDoesNotExist()
    {
        await SeedDbContextAsync();
        CategoryUpdateDto categoryDto = new()
        {
            CategoryName = "New category group.",
            CategoryGroupId = 2,
            Position = 5
        };

        OperationResult result = await _categoryService
            .UpdateCategoryAsync(1, 999, categoryDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryAsync_ShouldReturnFalseResult_WhenTryingToChangeCategoryGroupIdToNonExistingCategoryGroup()
    {
        await SeedDbContextAsync();
        CategoryUpdateDto categoryDto = new()
        {
            CategoryName = "New category group.",
            CategoryGroupId = 999,
            Position = 5
        };

        OperationResult result = await _categoryService
            .UpdateCategoryAsync(1, 1, categoryDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new CategoryService(dbContextMock, _imageService, _logger);
        CategoryUpdateDto categoryDto = new()
        {
            CategoryName = "New category group.",
            CategoryGroupId = 1,
            Position = 5
        };

        OperationResult result = await service
            .UpdateCategoryAsync(1, 1, categoryDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryImageAsync_ShouldAddImageToCategory()
    {
        await SeedDbContextAsync();
        ImageDto image = Image;
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryImageAsync(
            1, 1, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);
        Category? category = await _context.Categories
            .FirstOrDefaultAsync(cg => cg.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(category);
        Assert.Equal(image.Link, category.ImageLink);
    }
    [Fact]
    public async Task UpdateCategoryImageAsync_ShouldRemoveOldImage()
    {
        await SeedDbContextAsync();
        ImageDto image = Image;
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryImageAsync(
            2, 4, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        _imageService.Received().DeleteImage(Arg.Any<string>());
    }
    [Fact]
    public async Task UpdateCategoryImageAsync_ShouldReturnFalseResult_WhenCategoryDoesNotExist()
    {
        await SeedDbContextAsync();
        ImageDto image = Image;
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryImageAsync(
            1, 999, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryImageAsync_ShouldReturnFalseResult_WhenCategoryDoesNotExistInTheCategoryGroup()
    {
        await SeedDbContextAsync();
        ImageDto image = Image;
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryImageAsync(
            1, 4, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryImageAsync_ShouldReturnFalseResult_WhenImageWasNotUploaded()
    {
        await SeedDbContextAsync();
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(false));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.UpdateCategoryImageAsync(
            1, 1, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UpdateCategoryImageAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new CategoryService(dbContextMock, _imageService, _logger);
        _imageService.UploadImageAsync(Arg.Any<IFormFile>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<ImageDto>(true, Image));
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await service.UpdateCategoryImageAsync(
            1, 1, Substitute.For<IFormFile>(), "basePath", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteCategoryAsync_ShouldDeleteGroup()
    {
        await SeedDbContextAsync();
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.DeleteCategoryAsync(2, 5, CancellationToken.None);
        Category? category = await _context.Categories.FirstOrDefaultAsync(cg => cg.Id == 5);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Null(category);
        _imageService.Received().DeleteImage(Arg.Any<string>());
    }
    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFalseResult_WhenCategoryDoesNotExist()
    {
        await SeedDbContextAsync();
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.DeleteCategoryAsync(2, 999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFalseResult_WhenCategoryDoesNotExistInTheCategoryGroup()
    {
        await SeedDbContextAsync();
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.DeleteCategoryAsync(1, 5, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteCategoryAsync_ShouldReturnFalseResult_WhenCategoryIsNotEmpty()
    {
        await SeedDbContextAsync();
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await _categoryService.DeleteCategoryAsync(1, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteCategoryAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new CategoryService(dbContextMock, _imageService, _logger);
        _imageService.DeleteImage(Arg.Any<string>()).Returns(new OperationResult(true));

        OperationResult result = await service.DeleteCategoryAsync(2, 5, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }

    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        await context.CategoryGroups.AddRangeAsync(CategoryGroups);
        await context.Categories.AddRangeAsync(Categories);
        await context.Products.AddRangeAsync(Products);
        await context.SaveChangesAsync();
    }

    private static List<CategoryGroup> CategoryGroups =>
    [
        new()
        {
            Id = 1,
            Name = "Category group 1"
        },
        new()
        {
            Id = 2,
            Name = "Category group 2",
            ImageLink = "links/categoryGroup2",
            ImageFilePath = "wwwroot/links/categoryGroup2"
        },
        new()
        {
            Id = 3,
            Name = "Category group 3"
        }
    ];
    private static List<Category> Categories =>
    [
        new()
        {
            Id = 1,
            Name = "Category 1",
            CategoryGroupId = 1
        },
        new()
        {
            Id = 2,
            Name = "Category 2",
            CategoryGroupId = 1
        },
        new()
        {
            Id = 3,
            Name = "Category 3",
            CategoryGroupId = 2
        },
        new()
        {
            Id = 4,
            Name = "Category 4",
            CategoryGroupId = 2,
            ImageLink = "links/category4",
            ImageFilePath = "wwwroot/links/category4"
        },
        new()
        {
            Id = 5,
            Name = "Category 5",
            CategoryGroupId = 2
        }
    ];
    private static List<Product> Products =>
    [
        new()
        {
            Id = 1,
            CategoryId = 1,
            Name = "Product 1"
        },
        new()
        {
            Id = 2,
            CategoryId = 2,
            Name = "Product 2",
        },
        new()
        {
            Id = 3,
            CategoryId = 3,
            Name = "Product 3"
        },
        new()
        {
            Id = 4,
            CategoryId = 4,
            Name = "Product 4",
        }
    ];

    private static ImageDto Image => new()
    {
        FilePath = "test/path/1",
        Link = "test/link/1"
    };
}
