using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services;

public class ProductService(
    HmDbContext context,
    IImageService imageService,
    ILogger<ProductService> logger
    ) : IProductService
{
    public async Task<IEnumerable<ProductDto>> GetProductsAsync(int? categoryId, string? name,
        bool sortByPrice, bool sortByRating, bool sortAsc, CancellationToken cancellationToken)
    {
        IQueryable<Product> products = context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Feedbacks);
        if (categoryId != null)
        {
            products = products.Where(p => p.Category.Id == categoryId);
        }
        if (!string.IsNullOrWhiteSpace(name))
        {
            products = products.Where(p => p.Name.ToLower().Contains(name.ToLower()));
        }
        if (sortByPrice)
        {
            products = sortAsc
                ? products.OrderBy(p => p.Price)
                : products.OrderByDescending(p => p.Price);
        }
        else if (sortByRating)
        {
            products = sortAsc
                ? products.OrderBy(p => p.Rating)
                : products.OrderByDescending(p => p.Price);
        }
        var productsDto = new List<ProductDto>();
        foreach (var product in await products.ToListAsync(cancellationToken))
        {
            productsDto.Add(product.ToProductDto());
        }
        return productsDto;
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return null;
        }
        return product.ToProductDto();
    }

    public async Task<OperationResult<ProductDto>> CreateProductAsync(ProductCreateUpdateDto productDto, CancellationToken cancellationToken)
    {
        Category? category = await context.Categories
            .FirstOrDefaultAsync(c => c.Name == productDto.Category, cancellationToken);
        if (category == null)
        {
            return new OperationResult<ProductDto>(false, $"Category {productDto.Category} does not exist. " +
                $"Create the category first or specify another category.");
        }
        Product product = new()
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Category = category,
            Price = productDto.Price,
            StockQuantity = productDto.StockQuantity,
        };
        try
        {
            await context.Products.AddAsync(product, cancellationToken);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<ProductDto>(true, product.ToProductDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error has occurred while creating product {@product}", product);
            return new OperationResult<ProductDto>(false, "The product has not been created.");
        }
    }

    public async Task<OperationResult<ProductDto>> UpdateProductAsync(int productId,
        ProductCreateUpdateDto productDto, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Category)
            .Include(p => p.Images)
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult<ProductDto>(false, "Product with such an id does not exist.");
        }
        Category? category = await context.Categories
            .FirstOrDefaultAsync(c => c.Name == productDto.Category, cancellationToken);
        if (category == null)
        {
            return new OperationResult<ProductDto>(false, $"Category {productDto.Category} does not exist. " +
                $"Create the category first or specify another category.");
        }

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Category = category;
        product.Price = productDto.Price;
        product.StockQuantity = productDto.StockQuantity;

        try
        {
            context.Products.Update(product);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<ProductDto>(true, product.ToProductDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating product {@product}", product);
            return new OperationResult<ProductDto>(false, "The product has not been updated.");
        }
    }

    public async Task<OperationResult> UploadProductImagesAsync(int productId, IFormFile[] images,
        string baseUrlPath, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult(false, "The product does not exist");
        }

        string savePath = $"images/products/{productId}";
        OperationResult<List<ImageDto>> result = await imageService
            .UploadImagesAsync(images, baseUrlPath, savePath, cancellationToken);

        if(!result.Succeeded || result.Payload == null || result.Payload.Count == 0)
        {
            return new OperationResult<List<string>>(false, $"No images were added. {result.Message}");
        }

        foreach(ImageDto imageDto in result.Payload)
        {
            ProductImage productImage = new()
            {
                FilePath = imageDto.FilePath,
                Link = imageDto.Link
            };
            product.Images.Add(productImage);
        }
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            string message = $"{result.Payload.Count} images were added to the product {product.Name}.";
            message += images.Length > result.Payload.Count
                ? $" {images.Length - result.Payload.Count} images were not added. {result.Message}" : "";
            return new OperationResult(true, message);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while uploading images for the product {@product}", product);
            return new OperationResult(false, "Images was not added to the product.");
        }
    }

    public async Task<OperationResult> DeleteProductImageAsync(int productId, int imageId, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Images)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult(false, "The product does not exist");
        }
        ProductImage? image = product.Images.Find(i => i.Id == imageId);
        if (image == null)
        {
            return new OperationResult(false, $"The product {product.Name} does not contain the image with Id {imageId}");
        }

        try
        {
            imageService.DeleteImage(image.FilePath);
            product.Images.Remove(image);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true, "The image was removed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while removing image {@image}", image);
            return new OperationResult(false, "The image was not removed.");
        }
    }

    public async Task<OperationResult> DeleteProductAsync(int productId, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult(false, "Product with such an id does not exist.");
        }
        try
        {
            foreach(var image in product.Images)
            {
                imageService.DeleteImage(image.FilePath);
            }

            context.Products.Remove(product);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting product {@product}", product);
            return new OperationResult(false, "The product has not been deleted.");
        }
    }


    public async Task<OperationResult> AddFeedbackAsync(int productId, string? userId, ProductFeedbackCreateDto feedbackDto, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            return new OperationResult(false, "Product with such an id does not exist.");
        }
        ProductFeedback feedback = new()
        {
            ProductId = productId,
            UserId = userId,
            Created = DateTimeOffset.UtcNow,
            Rating = feedbackDto.Rating,
            Review = feedbackDto.Review
        };
        try
        {
            product.Feedbacks.Add(feedback);
            product.Rating = (product.Rating * product.TimesRated + feedbackDto.Rating) / (decimal)++product.TimesRated;
            context.Update(product);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding feedback {@feedback} to the product {@product}",
                feedback, product);
            return new OperationResult(false, "The feedback has not been added.");
        }
    }

    public async Task<OperationResult<IEnumerable<ProductFeedback>>> GetProductFeedbackAsync(int productId, CancellationToken cancellationToken)
    {
        var product = await context.Products
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult<IEnumerable<ProductFeedback>>(false, "Product with such an id does not exist.");
        }

        return new OperationResult<IEnumerable<ProductFeedback>>(true, product.Feedbacks);
    }

    public async Task<IEnumerable<ProductFeedback>> GetAllProductsFeedbackAsync(
        string? category, CancellationToken cancellationToken)
    {
        IQueryable<Product> products = context.Products
            .Include(p => p.Category)
            .Include(p => p.Feedbacks);
        if (category != null)
        {
            products = products.Where(p => p.Category.Name == category);
        }
        return await products
            .SelectMany(p => p.Feedbacks)
            .OrderByDescending(f => f.Created)
            .ToListAsync(cancellationToken);
    }
}
