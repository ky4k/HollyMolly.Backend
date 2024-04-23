using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Products;
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

public class ProductServiceTests
{
    private readonly HmDbContext _context;
    private readonly ProductService _productService;
    private readonly IImageService _imageService;
    private readonly ILogger<ProductService> _logger;
    public ProductServiceTests()
    {
        _imageService = Substitute.For<IImageService>();
        _logger = Substitute.For<ILogger<ProductService>>();
        _context = ServiceHelper.GetTestDbContext();
        _productService = new ProductService(_context, _imageService, _logger);
    }

    [Fact]
    public async Task GetProductsAsync_ShouldReturnAllProducts_WhenNoFilterWereSpecified()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(null, null, null, false, false, false, false, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(8, products.Count());
    }
    [Fact]
    public async Task GetProductsAsync_ShouldReturnProductsOfTheCategoryGroup_WhenCategoryGroupSpecified()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(1, null, null, false, false, false, false, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(4, products.Count());
    }
    [Fact]
    public async Task GetProductsAsync_ShouldReturnProductsOfTheCategory_WhenCategorySpecified()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(1, 1, null, false, false, false, false, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(2, products.Count());
    }
    [Theory]
    [InlineData("Product", 8)]
    [InlineData("product", 8)]
    [InlineData("5", 1)]
    public async Task GetProductsAsync_ShouldReturnProductsThatContainName_WhenNameSpecified(string name, int expected)
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(null, null, name, false, false, false, false, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(expected, products.Count());
    }
    [Fact]
    public async Task GetProductsAsync_ShouldFilterProductsInNewCollection_WhenIsNewCollectionTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(null, null, null, true, false, false, false, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(4, products.Count());
    }
    [Fact]
    public async Task GetProductsAsync_ShouldSortProductsByPriceAfterDiscountInAscendingOrder_WhenSortByPriceTrueAndSortAscTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(null, null, null, false, true, false, true, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(1, products.First().Id);
    }
    [Fact]
    public async Task GetProductsAsync_ShouldSortProductsByPriceAfterDiscountInDescendingOrder_WhenSortByPriceTrueAndSortAscFalse()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(null, null, null, false, true, false, false, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(8, products.First().Id);
    }
    [Fact]
    public async Task GetProductsAsync_ShouldSortProductsByRatingInAscendingOrder_WhenSortByRatingTrueAndSortAscTrue()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(null, null, null, false, false, true, true, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(-0.8m, products.First().Rating);
    }
    [Fact]
    public async Task GetProductsAsync_ShouldSortProductsByRatingInDescendingOrder_WhenSortByRatingTrueAndSortAscFalse()
    {
        await SeedDbContextAsync();

        IEnumerable<ProductDto> products = await _productService
            .GetProductsAsync(null, null, null, false, false, true, false, CancellationToken.None);

        Assert.NotNull(products);
        Assert.Equal(0.8m, products.First().Rating);
    }
    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnTheCorrectProduct()
    {
        await SeedDbContextAsync();

        ProductDto? product = await _productService
            .GetProductByIdAsync(3, CancellationToken.None);

        Assert.NotNull(product);
        Assert.Equal(3, product.Id);
    }
    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnAllProductDetails()
    {
        await SeedDbContextAsync();

        ProductDto? product = await _productService
            .GetProductByIdAsync(3, CancellationToken.None);

        Assert.NotNull(product);
        Assert.Single(product.Feedbacks);
        Assert.Equal(2, product.ProductsInstances.Count);
    }
    [Fact]
    public async Task GetProductByIdAsync_ShouldReturnNull_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();

        ProductDto? product = await _productService
            .GetProductByIdAsync(999, CancellationToken.None);

        Assert.Null(product);
    }
    [Fact]
    public async Task CreateProductAsync_ShouldCreateProduct()
    {
        await _context.AddAsync(Category1);
        await _context.SaveChangesAsync();
        ProductCreateDto productCreateDto = new()
        {
            CategoryId = Category1.Id,
            Name = "Created product",
            Description = "Test description",
            ProductInstances =
            [
                new()
                {
                    StockQuantity = 100,
                    Price = 75,
                },
                new()
                {
                    StockQuantity = 50,
                    Price = 200,
                },
                new()
                {
                    StockQuantity = 10,
                    Price = 500,
                }
            ]
        };

        OperationResult<ProductDto> result = await _productService
            .CreateProductAsync(productCreateDto, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(3, result.Payload.ProductsInstances.Count);
    }
    [Fact]
    public async Task CreateProductAsync_ShouldReturnFalseResult_WhenCategoryDoesNotExist()
    {
        ProductCreateDto productCreateDto = new()
        {
            CategoryId = Category1.Id,
            Name = "Created product",
            Description = "Test description",
            ProductInstances =
            [
                new()
                {
                    StockQuantity = 100,
                    Price = 75,
                }
            ]
        };

        OperationResult<ProductDto> result = await _productService
            .CreateProductAsync(productCreateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task CreateProductAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await dbContextMock.AddAsync(Category1);
        await dbContextMock.SaveChangesAsync();
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);
        ProductCreateDto productCreateDto = new()
        {
            CategoryId = Category1.Id,
            Name = "Created product",
            Description = "Test description",
            ProductInstances = []
        };

        OperationResult<ProductDto> result = await service
            .CreateProductAsync(productCreateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UpdateProductAsync_ShouldUpdateProduct()
    {
        await SeedDbContextAsync();
        ProductUpdateDto productUpdateDto = new()
        {
            Name = "Update",
            CategoryId = 2,
        };

        OperationResult<ProductDto> result = await _productService
            .UpdateProductAsync(1, productUpdateDto, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(productUpdateDto.Name, result.Payload.Name);
        Assert.Equal(productUpdateDto.CategoryId, result.Payload.CategoryId);
    }
    [Fact]
    public async Task UpdateProductAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();
        ProductUpdateDto productUpdateDto = new()
        {
            Name = "Update",
            CategoryId = 2,
        };

        OperationResult<ProductDto> result = await _productService
            .UpdateProductAsync(999, productUpdateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UpdateProductAsync_ShouldNotUpdateProduct_WhenNewCategoryDoesNotExist()
    {
        await SeedDbContextAsync();
        ProductUpdateDto productUpdateDto = new()
        {
            Name = "Update",
            CategoryId = 999,
        };

        OperationResult<ProductDto> result = await _productService
            .UpdateProductAsync(1, productUpdateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UpdateProductAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);
        ProductUpdateDto productUpdateDto = new()
        {
            Name = "Update",
            CategoryId = 2,
        };

        OperationResult<ProductDto> result = await service
            .UpdateProductAsync(1, productUpdateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task AddProductInstanceToProductAsync_ShouldAddProductInstance()
    {
        await SeedDbContextAsync();
        ProductInstanceCreateDto productInstanceCreateDto = new()
        {
            Color = "New color",
            StockQuantity = 5,
            Price = 10
        };

        OperationResult<ProductInstanceDto> result = await _productService
            .AddProductInstanceToProductAsync(1, productInstanceCreateDto, CancellationToken.None);
        ProductInstance? productInstance = (await _context.Products.FirstOrDefaultAsync(p => p.Id == 1))?
            .ProductInstances.First(pi => pi.Color == productInstanceCreateDto.Color
                && pi.StockQuantity == productInstanceCreateDto.StockQuantity
                && pi.Price == productInstanceCreateDto.Price);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(productInstanceCreateDto.Color, result.Payload.Color);
        Assert.NotNull(productInstance);
    }
    [Fact]
    public async Task AddProductInstanceToProductAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();
        ProductInstanceCreateDto productInstanceCreateDto = new()
        {
            Color = "New color",
            StockQuantity = 5,
            Price = 10
        };

        OperationResult<ProductInstanceDto> result = await _productService
            .AddProductInstanceToProductAsync(999, productInstanceCreateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task AddProductInstanceToProductAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);
        ProductInstanceCreateDto productInstanceCreateDto = new()
        {
            Color = "New color",
            StockQuantity = 5,
            Price = 10
        };

        OperationResult<ProductInstanceDto> result = await service
            .AddProductInstanceToProductAsync(1, productInstanceCreateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UpdateProductInstanceAsync_ShouldUpdateProductInstance()
    {
        await SeedDbContextAsync();

        ProductInstanceCreateDto productInstanceCreate = new()
        {
            AbsoluteDiscount = 10,
            PercentageDiscount = 10,
            Color = "Red",
            Size = "M",
            IsNewCollection = true,
            Material = "Cotton",
            SKU = "CMR",
            Status = "New status",
            StockQuantity = 10,
            Price = 50
        };

        OperationResult<ProductInstanceDto> result = await _productService
            .UpdateProductInstanceAsync(1, 1, productInstanceCreate, CancellationToken.None);
        ProductInstance? productInstance = await _context.Products.SelectMany(p => p.ProductInstances)
            .FirstOrDefaultAsync(pi => pi.ProductId == 1 && pi.Id == 1
                && pi.Color == productInstanceCreate.Color && pi.SKU == productInstanceCreate.SKU);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equivalent(productInstanceCreate, result.Payload);
        Assert.NotNull(productInstance);
    }
    [Fact]
    public async Task UpdateProductInstanceAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();

        ProductInstanceCreateDto productInstanceCreate = new()
        {
            StockQuantity = 10,
            Price = 50
        };

        OperationResult<ProductInstanceDto> result = await _productService
            .UpdateProductInstanceAsync(999, 1, productInstanceCreate, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UpdateProductInstanceAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotExist()
    {
        await SeedDbContextAsync();

        ProductInstanceCreateDto productInstanceCreate = new()
        {
            StockQuantity = 10,
            Price = 50
        };

        OperationResult<ProductInstanceDto> result = await _productService
            .UpdateProductInstanceAsync(1, 999, productInstanceCreate, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UpdateProductInstanceAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotBelongToTheSpecifiedProduct()
    {
        await SeedDbContextAsync();

        ProductInstanceCreateDto productInstanceCreate = new()
        {
            StockQuantity = 10,
            Price = 50
        };

        OperationResult<ProductInstanceDto> result = await _productService
            .UpdateProductInstanceAsync(2, 1, productInstanceCreate, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UpdateProductInstanceAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);
        ProductInstanceCreateDto productInstanceCreate = new()
        {
            StockQuantity = 10,
            Price = 50
        };

        OperationResult<ProductInstanceDto> result = await service
            .UpdateProductInstanceAsync(1, 1, productInstanceCreate, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task DeleteProductInstanceAsync_ShouldDeleteProductInstance()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService
            .DeleteProductInstanceAsync(1, 1, CancellationToken.None);
        ProductInstance? productInstance = await _context.Products.SelectMany(p => p.ProductInstances)
            .FirstOrDefaultAsync(pi => pi.ProductId == 1 && pi.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Null(productInstance);
    }
    [Fact]
    public async Task DeleteProductInstanceAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService
            .DeleteProductInstanceAsync(999, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductInstanceAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService
            .DeleteProductInstanceAsync(1, 999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductInstanceAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotBelongToTheSpecifiedProduct()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService
            .DeleteProductInstanceAsync(2, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductInstanceAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);

        OperationResult result = await service.DeleteProductInstanceAsync(1, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UploadProductImagesAsync_ShouldAddImageLinkToTheProductInstance()
    {
        await SeedDbContextAsync();
        _imageService.UploadImagesAsync(Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<List<ImageDto>>(true, Images));

        OperationResult<ProductInstanceDto> result = await _productService
            .UploadProductImagesAsync(1, 1, [], "", CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Payload.Images.Count);
    }
    [Fact]
    public async Task UploadProductImagesAsync_ShouldAddImageLinkToTheProductInstance_WhenItAlreadyHaveImages()
    {
        await SeedDbContextAsync();
        _imageService.UploadImagesAsync(Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<List<ImageDto>>(true, Images));

        OperationResult<ProductInstanceDto> result = await _productService
            .UploadProductImagesAsync(5, 9, [], "", CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Payload.Images.Count);
    }
    [Fact]
    public async Task UploadProductImagesAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();
        _imageService.UploadImagesAsync(Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<List<ImageDto>>(true, Images));

        OperationResult<ProductInstanceDto> result = await _productService
            .UploadProductImagesAsync(999, 1, [], "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UploadProductImagesAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotExist()
    {
        await SeedDbContextAsync();
        _imageService.UploadImagesAsync(Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<List<ImageDto>>(true, Images));

        OperationResult<ProductInstanceDto> result = await _productService
            .UploadProductImagesAsync(1, 999, [], "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UploadProductImagesAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotExistInTheSpecifiedProduct()
    {
        await SeedDbContextAsync();
        _imageService.UploadImagesAsync(Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<List<ImageDto>>(true, Images));

        OperationResult<ProductInstanceDto> result = await _productService
            .UploadProductImagesAsync(2, 1, [], "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UploadProductImagesAsync_ShouldReturnFalseResult_WhenImageWasNotUploaded()
    {
        await SeedDbContextAsync();
        _imageService.UploadImagesAsync(Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<List<ImageDto>>(false, Images));

        OperationResult<ProductInstanceDto> result = await _productService
            .UploadProductImagesAsync(1, 1, [], "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UploadProductImagesAsync_ShouldReturnFalseResult_WhenNoImageWasUploaded()
    {
        await SeedDbContextAsync();
        _imageService.UploadImagesAsync(Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<List<ImageDto>>(true, new List<ImageDto>()));

        OperationResult<ProductInstanceDto> result = await _productService
            .UploadProductImagesAsync(1, 1, [], "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task UploadProductImagesAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        _imageService.UploadImagesAsync(Arg.Any<IFormFile[]>(), Arg.Any<string>(), Arg.Any<string>(),
            Arg.Any<CancellationToken>()).Returns(new OperationResult<List<ImageDto>>(true, Images));
        var service = new ProductService(dbContextMock, _imageService, _logger);

        OperationResult<ProductInstanceDto> result = await service
            .UploadProductImagesAsync(1, 1, [], "", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RearrangeProductImagesAsync_ShouldChangeImagesPosition()
    {
        await SeedDbContextAsync();
        List<ProductImageRearrangeDto> productImageRearrangeDtos =
        [
            new() { Id = 3, Position = 1 },
            new() { Id = 1, Position = 2 },
            new() { Id = 2, Position = 3 }
        ];

        OperationResult result = await _productService
            .RearrangeProductImagesAsync(5, 9, productImageRearrangeDtos, CancellationToken.None);
        List<ProductImage>? productImages = (await _context.Products
            .FirstOrDefaultAsync(p => p.Id == 5))?.ProductInstances.Find(pi => pi.Id == 9)?.Images;

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(productImages);
        Assert.Equal(1, productImages.First(pi => pi.Id == 3).Position);
        Assert.Equal(2, productImages.First(pi => pi.Id == 1).Position);
        Assert.Equal(3, productImages.First(pi => pi.Id == 2).Position);
    }
    [Fact]
    public async Task RearrangeProductImagesAsync_ShouldIgnoreChangesForImagesThatNotInProductInstance()
    {
        await SeedDbContextAsync();
        List<ProductImageRearrangeDto> productImageRearrangeDtos =
        [
            new() { Id = 5, Position = 1 },
            new() { Id = 1, Position = 4 },
            new() { Id = 6, Position = 3 }
        ];

        OperationResult result = await _productService
            .RearrangeProductImagesAsync(5, 9, productImageRearrangeDtos, CancellationToken.None);
        List<ProductImage>? productImages = (await _context.Products
            .FirstOrDefaultAsync(p => p.Id == 5))?.ProductInstances.Find(pi => pi.Id == 9)?.Images;

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(productImages);
        Assert.Equal(4, productImages.First(pi => pi.Id == 1).Position);
        Assert.Equal(2, productImages.First(pi => pi.Id == 2).Position);
        Assert.Equal(3, productImages.First(pi => pi.Id == 3).Position);
    }
    [Fact]
    public async Task RearrangeProductImagesAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();
        List<ProductImageRearrangeDto> productImageRearrangeDtos =
        [
            new() { Id = 3, Position = 1 },
            new() { Id = 1, Position = 2 },
            new() { Id = 2, Position = 3 }
        ];

        OperationResult result = await _productService
            .RearrangeProductImagesAsync(999, 9, productImageRearrangeDtos, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RearrangeProductImagesAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotExist()
    {
        await SeedDbContextAsync();
        List<ProductImageRearrangeDto> productImageRearrangeDtos =
        [
            new() { Id = 3, Position = 1 },
            new() { Id = 1, Position = 2 },
            new() { Id = 2, Position = 3 }
        ];

        OperationResult result = await _productService
            .RearrangeProductImagesAsync(5, 999, productImageRearrangeDtos, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RearrangeProductImagesAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotExistInTheSpecifiedProduct()
    {
        await SeedDbContextAsync();
        List<ProductImageRearrangeDto> productImageRearrangeDtos =
        [
            new() { Id = 3, Position = 1 },
            new() { Id = 1, Position = 2 },
            new() { Id = 2, Position = 3 }
        ];

        OperationResult result = await _productService
            .RearrangeProductImagesAsync(2, 9, productImageRearrangeDtos, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task RearrangeProductImagesAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);

        List<ProductImageRearrangeDto> productImageRearrangeDtos =
        [
            new() { Id = 3, Position = 1 },
            new() { Id = 1, Position = 2 },
            new() { Id = 2, Position = 3 }
        ];

        OperationResult result = await service
            .RearrangeProductImagesAsync(5, 9, productImageRearrangeDtos, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductImagesAsync_ShouldRemoveImagesLinks()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductImageAsync(5, 9, 1, CancellationToken.None);
        ProductImage? image = (await _context.Products.FirstOrDefaultAsync(p => p.Id == 5))
            ?.ProductInstances.Find(pi => pi.Id == 9)?.Images.Find(im => im.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Null(image);
    }
    [Fact]
    public async Task DeleteProductImagesAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductImageAsync(999, 9, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductImagesAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductImageAsync(5, 999, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductImagesAsync_ShouldReturnFalseResult_WhenProductInstanceDoesNotExistInTheProduct()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductImageAsync(2, 9, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductImagesAsync_ShouldReturnFalseResult_WhenProductImageDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductImageAsync(5, 9, 999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductImagesAsync_ShouldReturnFalseResult_WhenProductImageDoesNotExistInTheProductInstance()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductImageAsync(5, 10, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductImagesAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);

        OperationResult result = await service
            .DeleteProductImageAsync(5, 9, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteProduct()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductAsync(5, CancellationToken.None);
        Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == 5);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Null(product);
    }
    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteAllProductInstancesInProduct()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductAsync(5, CancellationToken.None);
        ProductInstance? productInstance = await _context.Products.SelectMany(p => p.ProductInstances)
            .FirstOrDefaultAsync(pi => pi.Id == 9);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Null(productInstance);
    }
    [Fact]
    public async Task DeleteProductAsync_ShouldDeleteAllProductImagesInAllProductInstancesInProduct()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductAsync(5, CancellationToken.None);
        ProductImage? productImage = await _context.Products.SelectMany(p => p.ProductInstances)
            .SelectMany(p => p.Images).FirstOrDefaultAsync(im => im.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Null(productImage);
    }
    [Fact]
    public async Task DeleteProductAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();

        OperationResult result = await _productService.DeleteProductAsync(999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);

        OperationResult result = await service.DeleteProductAsync(5, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task GetAllProductsFeedbackAsync_ShouldReturnAllFeedback()
    {
        await SeedDbContextAsync();
        IEnumerable<ProductFeedbackDto> productFeedbackDtos = await _productService
            .GetAllProductsFeedbackAsync(null, null, CancellationToken.None);

        Assert.NotNull(productFeedbackDtos);
        Assert.Equal(5, productFeedbackDtos.Count());
    }
    [Fact]
    public async Task GetAllProductsFeedbackAsync_ShouldReturnAllFeedbackInCategoryGroup_WhenSpecified()
    {
        await SeedDbContextAsync();
        IEnumerable<ProductFeedbackDto> productFeedbackDtos = await _productService
            .GetAllProductsFeedbackAsync(1, null, CancellationToken.None);

        Assert.NotNull(productFeedbackDtos);
        Assert.Equal(4, productFeedbackDtos.Count());
    }
    [Fact]
    public async Task GetAllProductsFeedbackAsync_ShouldReturnAllFeedbackInCategory_WhenSpecified()
    {
        await SeedDbContextAsync();
        IEnumerable<ProductFeedbackDto> productFeedbackDtos = await _productService
            .GetAllProductsFeedbackAsync(1, 1, CancellationToken.None);

        Assert.NotNull(productFeedbackDtos);
        Assert.Equal(3, productFeedbackDtos.Count());
    }
    [Fact]
    public async Task GetProductFeedbackAsync_ShouldReturnAllProductFeedback()
    {
        await SeedDbContextAsync();
        OperationResult<IEnumerable<ProductFeedbackDto>> result = await _productService
            .GetProductFeedbackAsync(1, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Payload.Count());
    }
    [Fact]
    public async Task GetProductFeedbackAsync_ShouldReturnEmptyCollection_WhenProductHasNoFeedback()
    {
        await SeedDbContextAsync();
        OperationResult<IEnumerable<ProductFeedbackDto>> result = await _productService
            .GetProductFeedbackAsync(8, CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Empty(result.Payload);
    }
    [Fact]
    public async Task GetProductFeedbackAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();
        OperationResult<IEnumerable<ProductFeedbackDto>> result = await _productService
            .GetProductFeedbackAsync(999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
        Assert.Null(result.Payload);
    }
    [Fact]
    public async Task AddFeedbackAsync_ShouldAddFeedbackToTheProduct()
    {
        await SeedDbContextAsync();
        ProductFeedbackCreateDto productFeedbackCreateDto = new()
        {
            AuthorName = "NewlyAdded",
            Rating = 1,
            Review = "Review"
        };

        OperationResult result = await _productService
            .AddFeedbackAsync(1, "1234-5678-9012-3456", productFeedbackCreateDto, CancellationToken.None);
        ProductFeedback? feedback = (await _context.Products.FirstOrDefaultAsync(p => p.Id == 1))
            ?.Feedbacks.Find(f => f.AuthorName == productFeedbackCreateDto.AuthorName);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(feedback);
    }
    [Fact]
    public async Task AddFeedbackAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();
        ProductFeedbackCreateDto productFeedbackCreateDto = new()
        {
            AuthorName = "NewlyAdded",
            Rating = 1,
            Review = "Review"
        };

        OperationResult result = await _productService
            .AddFeedbackAsync(999, "1234-5678-9012-3456", productFeedbackCreateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task AddFeedbackAsync_ShouldUpdateProductRating()
    {
        await SeedDbContextAsync();
        ProductFeedbackCreateDto productFeedbackCreateDto = new()
        {
            AuthorName = "NewlyAdded",
            Rating = 1,
            Review = "Review"
        };

        OperationResult result = await _productService
            .AddFeedbackAsync(5, "1234-5678-9012-3456", productFeedbackCreateDto, CancellationToken.None);
        Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == 5);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(product);
        Assert.Equal(-0.2m, product.Rating);
    }
    [Fact]
    public async Task AddFeedbackAsync_ShouldUpdateTimesRates()
    {
        await SeedDbContextAsync();
        ProductFeedbackCreateDto productFeedbackCreateDto = new()
        {
            AuthorName = "NewlyAdded",
            Rating = 1,
            Review = "Review"
        };

        OperationResult result = await _productService
            .AddFeedbackAsync(5, "1234-5678-9012-3456", productFeedbackCreateDto, CancellationToken.None);
        Product? product = await _context.Products.FirstOrDefaultAsync(p => p.Id == 5);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.NotNull(product);
        Assert.Equal(5, product.TimesRated);
    }
    [Fact]
    public async Task AddFeedbackAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);
        ProductFeedbackCreateDto productFeedbackCreateDto = new()
        {
            AuthorName = "NewlyAdded",
            Rating = 1,
            Review = "Review"
        };

        OperationResult result = await service
            .AddFeedbackAsync(1, "1234-5678-9012-3456", productFeedbackCreateDto, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductFeedbackAsync_ShouldDeleteProductFeedback()
    {
        await SeedDbContextAsync();
        OperationResult result = await _productService
            .DeleteProductFeedbackAsync(1, 1, CancellationToken.None);
        ProductFeedback? feedback = await _context.Products
            .SelectMany(p => p.Feedbacks).FirstOrDefaultAsync(f => f.Id == 1);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.Null(feedback);
    }
    [Fact]
    public async Task DeleteProductFeedbackAsync_ShouldReturnFalseResult_WhenProductDoesNotExist()
    {
        await SeedDbContextAsync();
        OperationResult result = await _productService
            .DeleteProductFeedbackAsync(999, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductFeedbackAsync_ShouldReturnFalseResult_WhenFeedbackDoesNotExist()
    {
        await SeedDbContextAsync();
        OperationResult result = await _productService
            .DeleteProductFeedbackAsync(1, 999, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductFeedbackAsync_ShouldReturnFalseResult_WhenFeedbackDoesNotExistInProduct()
    {
        await SeedDbContextAsync();
        OperationResult result = await _productService
            .DeleteProductFeedbackAsync(2, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task DeleteProductFeedbackAsync_ShouldHandleDatabaseErrors()
    {
        var dbContextMock = Substitute.ForPartsOf<HmDbContext>(ServiceHelper.GetTestDbContextOptions());
        await SeedDbContextAsync(dbContextMock);
        dbContextMock.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync<InvalidOperationException>();
        var service = new ProductService(dbContextMock, _imageService, _logger);

        OperationResult result = await service
            .DeleteProductFeedbackAsync(1, 1, CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    private async Task SeedDbContextAsync()
    {
        await SeedDbContextAsync(_context);
    }

    private static async Task SeedDbContextAsync(HmDbContext context)
    {
        await context.CategoryGroups.AddRangeAsync(CategoryGroup1, CategoryGroup2);
        await context.Categories.AddRangeAsync(Category1, Category2, Category3, Category4);
        await context.Products.AddRangeAsync(Product1, Product2, Product3, Product4,
            Product5, Product6, Product7, Product8);
        await context.SaveChangesAsync();
    }

    private static CategoryGroup CategoryGroup1 => new()
    {
        Id = 1,
        Name = "Category group 1"
    };

    private static CategoryGroup CategoryGroup2 => new()
    {
        Id = 2,
        Name = "Category group 2"
    };

    private static Category Category1 => new()
    {
        Id = 1,
        Name = "Category 1",
        CategoryGroupId = 1
    };
    private static Category Category2 => new()
    {
        Id = 2,
        Name = "Category 2",
        CategoryGroupId = 1
    };
    private static Category Category3 => new()
    {
        Id = 3,
        Name = "Category 3",
        CategoryGroupId = 2
    };
    private static Category Category4 => new()
    {
        Id = 4,
        Name = "Category 4",
        CategoryGroupId = 2
    };
    private static Product Product1 => new()
    {
        Id = 1,
        CategoryId = 1,
        Name = "Product 1",
        Description = "Description 1",
        Rating = 0.5m,
        TimesRated = 10,
        ProductInstances =
        [
            new()
            {
                Id = 1,
                StockQuantity = 100,
                Price = 60,
                AbsoluteDiscount = 10
            },
            new()
            {
                Id = 2,
                StockQuantity = 100,
                Price = 60,
                PercentageDiscount = 10,
                IsNewCollection = true
            }
        ],
        Feedbacks =
        [
            new()
            {
                Id = 1,
                AuthorName = "Author",
                Rating = 1,
                Review = "Review"
            },
            new()
            {
                Id = 2,
                AuthorName = "Author",
                Rating = 1,
                Review = "Review"
            }
        ],
        WishLists = [],
        ProductStatistics = []
    };
    private static Product Product2 => new()
    {
        Id = 2,
        CategoryId = 1,
        Name = "Product 2",
        Description = "Description 2",
        Rating = 0.6m,
        TimesRated = 10,
        ProductInstances =
        [
            new()
            {
                Id = 3,
                StockQuantity = 100,
                Price = 55,
                AbsoluteDiscount = 0
            },
            new()
            {
                Id = 4,
                StockQuantity = 100,
                Price = 70,
                PercentageDiscount = 10,
                IsNewCollection = true
            }
        ],
        Feedbacks =
        [
            new()
            {
                Id = 3,
                AuthorName = "Author",
                Rating = 1,
                Review = "Review"
            }
        ],
        WishLists = [],
        ProductStatistics = []
    };
    private static Product Product3 => new()
    {
        Id = 3,
        CategoryId = 2,
        Name = "Product 3",
        Description = "Description 3",
        Rating = 0.7m,
        TimesRated = 10,
        ProductInstances =
        [
            new()
            {
                Id = 5,
                StockQuantity = 100,
                Price = 80,
                AbsoluteDiscount = 10
            },
            new()
            {
                Id = 6,
                StockQuantity = 100,
                Price = 80,
                PercentageDiscount = 10
            }
        ],
        Feedbacks =
        [
            new() {
                Id = 4,
                AuthorName = "Author",
                Created = new DateTimeOffset(2024, 4, 22, 20, 0 , 0, TimeSpan.Zero),
                Rating = 1,
                Review = "Review"
            }
        ],
        WishLists = [],
        ProductStatistics = []
    };
    private static Product Product4 => new()
    {
        Id = 4,
        CategoryId = 2,
        Name = "Product 4",
        Description = "Description 4",
        Rating = 0.8m,
        TimesRated = 10,
        ProductInstances =
        [
            new()
            {
                Id = 7,
                StockQuantity = 100,
                Price = 90,
                AbsoluteDiscount = 10
            },
            new()
            {
                Id = 8,
                StockQuantity = 100,
                Price = 90,
                PercentageDiscount = 10
            }
        ],
        Feedbacks = [],
        WishLists = [],
        ProductStatistics = []
    };
    private static Product Product5 => new()
    {
        Id = 5,
        CategoryId = 3,
        Name = "Product 5",
        Description = "Description 5",
        Rating = -0.5m,
        TimesRated = 4,
        ProductInstances =
        [
            new()
            {
                Id = 9,
                StockQuantity = 100,
                Price = 100,
                AbsoluteDiscount = 10,
                Images =
                [
                    new()
                    {
                        Id = 1,
                        Position = 1,
                        Link = "test/link/name1",
                        FilePath = "test/path/name1"
                    },
                    new()
                    {
                        Id = 2,
                        Position = 2,
                        Link = "test/link/name1",
                        FilePath = "test/path/name1"
                    },
                    new()
                    {
                        Id = 3,
                        Position = 3,
                        Link = "test/link/name1",
                        FilePath = "test/path/name1"
                    }
                ]
            },
            new()
            {
                Id = 10,
                StockQuantity = 100,
                Price = 100,
                PercentageDiscount = 10,
                IsNewCollection = true
            }
        ],
        Feedbacks =
        [
            new()
            {
                Id = 5,
                AuthorName = "Author",
                Rating = 1,
                Review = "Review"
            }
        ],
        WishLists = [],
        ProductStatistics = []
    };
    private static Product Product6 => new()
    {
        Id = 6,
        CategoryId = 3,
        Name = "Product 6",
        Description = "Description 6",
        Rating = -0.6m,
        TimesRated = 10,
        ProductInstances =
        [
            new()
            {
                Id = 11,
                StockQuantity = 100,
                Price = 110,
                AbsoluteDiscount = 10
            },
            new()
            {
                Id = 12,
                StockQuantity = 100,
                Price = 110,
                PercentageDiscount = 10,
                IsNewCollection = true
            }
        ],
        Feedbacks = [],
        WishLists = [],
        ProductStatistics = []
    };
    private static Product Product7 => new()
    {
        Id = 7,
        CategoryId = 4,
        Name = "Product 7",
        Description = "Description 7",
        Rating = -0.7m,
        TimesRated = 10,
        ProductInstances =
        [
            new()
            {
                Id = 13,
                StockQuantity = 100,
                Price = 120,
                AbsoluteDiscount = 10
            },
            new()
            {
                Id = 14,
                StockQuantity = 100,
                Price = 130,
                PercentageDiscount = 10
            }
        ],
        Feedbacks = [],
        WishLists = [],
        ProductStatistics = []
    };
    private static Product Product8 => new()
    {
        Id = 8,
        CategoryId = 4,
        Name = "Product 8",
        Description = "Description 8",
        Rating = -0.8m,
        TimesRated = 10,
        ProductInstances =
        [
            new()
            {
                Id = 15,
                StockQuantity = 100,
                Price = 130,
                AbsoluteDiscount = 10
            },
            new()
            {
                Id = 16,
                StockQuantity = 100,
                Price = 120,
                PercentageDiscount = 10
            }
        ],
        Feedbacks = [],
        WishLists = [],
        ProductStatistics = []
    };

    private static List<ImageDto> Images =>
    [
        new()
        {
            FilePath = "test/path/1",
            Link = "test/link/1"
        },
        new()
        {
            FilePath = "test/path/2",
            Link = "test/link/2"
        }
    ];
}
