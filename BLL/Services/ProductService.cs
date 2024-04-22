using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.Products;
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
    public async Task<IEnumerable<ProductDto>> GetProductsAsync(int? categoryGroupId, int? categoryId,
        string? name, bool onlyNewCollection, bool sortByPrice, bool sortByRating, bool sortAsc,
        CancellationToken cancellationToken)
    {
        IQueryable<Product> products = context.Products
            .Include(p => p.ProductInstances)
                .ThenInclude(pi => pi.Images)
            .Include(p => p.Category)
            .Include(p => p.Feedbacks)
            .AsSplitQuery()
            .AsNoTracking();
        if (onlyNewCollection)
        {
            products = products.Where(p => p.ProductInstances.Any(pi => pi.IsNewCollection));
        }
        if (categoryId != null)
        {
            products = products.Where(p => p.Category.Id == categoryId);
        }
        else if (categoryGroupId != null)
        {
            List<int> categoriesId = await context.Categories
                .Where(c => c.CategoryGroupId == categoryGroupId)
                .Select(c => c.Id)
                .ToListAsync(cancellationToken);
            products = products.Where(p => categoriesId.Contains(p.CategoryId));
        }
        if (!string.IsNullOrWhiteSpace(name))
        {
            products = FilterByName(products, name);
        }
        if (sortByPrice)
        {
            products = sortAsc
                ? products.OrderBy(p => p.ProductInstances.Select(pi => pi.Price).Min())
                : products.OrderByDescending(p => p.ProductInstances.Select(pi => pi.Price).Max());
        }
        else if (sortByRating)
        {
            products = sortAsc
                ? products.OrderBy(p => p.Rating)
                : products.OrderByDescending(p => p.Rating);
        }
        return await products.Select(p => p.ToProductDto()).ToListAsync(cancellationToken);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1862:Use the 'StringComparison' method overloads to perform case-insensitive string comparisons",
        Justification = "https://github.com/dotnet/efcore/issues/20995#issuecomment-631358780 EF Core does not translate the overload that accepts StringComparison.InvariantCultureIgnoreCase (or any other StringComparison).")]
    private static IQueryable<Product> FilterByName(IQueryable<Product> products, string name)
    {
        return products.Where(p => p.Name.ToLower().Contains(name.ToLower()));
    }

    public async Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.ProductInstances)
                .ThenInclude(pi => pi.Images)
            .Include(p => p.Category)
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        return product?.ToProductDto();
    }

    public async Task<OperationResult<ProductDto>> CreateProductAsync(ProductCreateDto productDto, CancellationToken cancellationToken)
    {
        Category? category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == productDto.CategoryId, cancellationToken);
        if (category == null)
        {
            return new OperationResult<ProductDto>(false, $"Category with ID {productDto.CategoryId} " +
                $"does not exist. Create the category first or specify another category.");
        }

        Product product = new()
        {
            Name = productDto.Name,
            Description = productDto.Description,
            Category = category,
            ProductInstances = productDto.ProductInstances .Select(pid => pid.ToProductInstance()).ToList()
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
        ProductUpdateDto productDto, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.ProductInstances)
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult<ProductDto>(false, "Product with such an id does not exist.");
        }
        Category? category = await context.Categories
            .FirstOrDefaultAsync(c => c.Id == productDto.CategoryId, cancellationToken);
        if (category == null)
        {
            return new OperationResult<ProductDto>(false, $"Category with id {productDto.CategoryId} " +
                $"does not exist. Create the category first or specify another category.");
        }

        product.Name = productDto.Name;
        product.Description = productDto.Description;
        product.Category = category;

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

    public async Task<OperationResult<ProductInstanceDto>> AddProductInstanceToProduct(int productId,
        ProductInstanceCreateDto productInstanceDto, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.ProductInstances)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult<ProductInstanceDto>(false, $"The product with id {productId} does not exist");
        }
        ProductInstance productInstance = productInstanceDto.ToProductInstance();
        try
        {
            product.ProductInstances.Add(productInstance);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<ProductInstanceDto>(true, productInstance.ToProductInstanceDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding new product instance for the product with {id}: " +
                "{@productInstance}", productId, productInstance);
            return new OperationResult<ProductInstanceDto>(false, "Product instance was not added");
        }
    }

    public async Task<OperationResult<ProductInstanceDto>> UpdateProductInstanceAsync(int productId, int productInstanceId,
        ProductInstanceCreateDto productInstanceDto, CancellationToken cancellationToken)
    {
        OperationResult<ProductInstance> instanceResult = 
            await GetProductInstanceAsync(productId, productInstanceId, cancellationToken);
        if (!instanceResult.Succeeded || instanceResult.Payload == null)
        {
            return new OperationResult<ProductInstanceDto>(false, instanceResult.Message ?? "");
        }
        ProductInstance productInstance = instanceResult.Payload;
        UpdateProductInstanceProperties(productInstance, productInstanceDto);
        try
        {
            context.Update(productInstance);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<ProductInstanceDto>(true, productInstance.ToProductInstanceDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while updating produceInstance {@productInstance}", productInstance);
            return new OperationResult<ProductInstanceDto>(false, "The product instance has not been updated.");
        }
    }

    private static void UpdateProductInstanceProperties(ProductInstance toUpdate, ProductInstanceCreateDto updated)
    {
        toUpdate.StockQuantity = updated.StockQuantity;
        toUpdate.Price = updated.Price;
        toUpdate.Status = updated.Status;
        toUpdate.IsNewCollection = updated.IsNewCollection;
        toUpdate.SKU = updated.SKU;
        toUpdate.Color = updated.Color;
        toUpdate.Size = updated.Size;
        toUpdate.Material = updated.Material;
        toUpdate.AbsoluteDiscount = updated.AbsoluteDiscount;
        toUpdate.PercentageDiscount = updated.PercentageDiscount;
    }

    public async Task<OperationResult> DeleteProductInstanceAsync(int productId, int productInstanceId,
        CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.ProductInstances)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult(false, $"The product with id {productId} does not exist");
        }
        ProductInstance? productInstance = product.ProductInstances.Find(pi => pi.Id == productInstanceId);
        if (productInstance == null)
        {
            return new OperationResult(false, $"The product with id {productId} does not " +
                $"contain the product instance with id {productInstanceId}");
        }
        try
        {
            product.ProductInstances.Remove(productInstance);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting product instance {@productInstance}", productInstance);
            return new OperationResult(false, "Product instance was not deleted.");
        }
    }

    public async Task<OperationResult<ProductInstanceDto>> UploadProductImagesAsync(int productId,
        int productInstanceId, IFormFile[] images, string baseUrlPath, CancellationToken cancellationToken)
    {
        OperationResult<ProductInstance> instanceResult = await GetProductInstanceAsync(
            productId, productInstanceId, cancellationToken);
        if (!instanceResult.Succeeded || instanceResult.Payload == null)
        {
            return new OperationResult<ProductInstanceDto>(false, instanceResult.Message ?? "");
        }
        ProductInstance productInstance = instanceResult.Payload;

        string savePath = $"images/products/{productInstance.Id}";
        OperationResult<List<ImageDto>> result = await imageService
            .UploadImagesAsync(images, baseUrlPath, savePath, cancellationToken);

        if (!result.Succeeded || result.Payload == null || result.Payload.Count == 0)
        {
            return new OperationResult<ProductInstanceDto>(false, $"No images were added. {result.Message}");
        }
        productInstance.Images ??= [];
        int startPosition = productInstance.Images.Select(i => i.Position).DefaultIfEmpty(0).Max();
        foreach (ImageDto imageDto in result.Payload)
        {
            ProductImage productImage = new()
            {
                FilePath = imageDto.FilePath,
                Position = ++startPosition,
                Link = imageDto.Link
            };
            productInstance.Images.Add(productImage);
        }
        try
        {
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<ProductInstanceDto>(true, productInstance.ToProductInstanceDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while uploading images" +
                " for the productInstance {@productInstance}", productInstance);
            return new OperationResult<ProductInstanceDto>(false, "Images was not added to the product.");
        }
    }

    public async Task<OperationResult> RearrangeProductImagesAsync(int productId,
        int productInstanceId, List<ProductImageRearrangeDto> imageRearrangesDto,
        CancellationToken cancellationToken)
    {
        OperationResult<ProductInstance> instanceResult = await GetProductInstanceAsync(
            productId, productInstanceId, cancellationToken);
        if (!instanceResult.Succeeded || instanceResult.Payload == null)
        {
            return instanceResult;
        }
        ProductInstance productInstance = instanceResult.Payload;
        foreach (ProductImage image in productInstance.Images)
        {
            ProductImageRearrangeDto? rearrangeDto = imageRearrangesDto.Find(ird => ird.Id == image.Id);
            if (rearrangeDto != null)
            {
                image.Position = rearrangeDto.Position;
            }
        }
        try
        {
            context.Update(productInstance);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while rearranging images for the productInstance " +
                "{@productInstance}", productInstance);
            return new OperationResult(false, "Images were not rearranged");
        }
    }

    public async Task<OperationResult> DeleteProductImageAsync(int productId,
        int productInstanceId, int imageId, CancellationToken cancellationToken)
    {
        OperationResult<ProductInstance> instanceResult = await GetProductInstanceAsync(
            productId, productInstanceId, cancellationToken);
        if (!instanceResult.Succeeded || instanceResult.Payload == null)
        {
            return instanceResult;
        }
        ProductInstance productInstance = instanceResult.Payload;
        ProductImage? image = productInstance.Images.Find(i => i.Id == imageId);
        if (image == null)
        {
            return new OperationResult(false, $"The product instance with id {productInstance.Id}" +
                $" does not contain the image with Id {imageId}");
        }

        try
        {
            imageService.DeleteImage(image.FilePath);
            productInstance.Images.Remove(image);
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
            .Include(p => p.ProductInstances)
                .ThenInclude(pi => pi.Images)
            .Include(p => p.Category)
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult(false, "Product with such an id does not exist.");
        }
        try
        {
            foreach (ProductInstance productInstance in product.ProductInstances)
            {
                foreach (ProductImage image in productInstance.Images)
                {
                    imageService.DeleteImage(image.FilePath);
                }
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

    public async Task<IEnumerable<ProductFeedbackDto>> GetAllProductsFeedbackAsync(
        int? categoryGroupId, int? categoryId, CancellationToken cancellationToken)
    {
        IQueryable<Product> products = context.Products
            .Include(p => p.Feedbacks);
        if (categoryId != null)
        {
            products = products.Where(p => p.CategoryId == categoryId);
        }
        else if (categoryGroupId != null)
        {
            CategoryGroup? categoryGroup = await context.CategoryGroups
                .Include(cg => cg.Categories)
                .FirstOrDefaultAsync(cg => cg.Id == categoryGroupId, cancellationToken);
            List<int> categoriesIds = [];
            if (categoryGroup != null)
            {
                categoriesIds = categoryGroup.Categories.Select(c => c.Id).ToList();
            }
            products = products.Where(p => categoriesIds.Contains(p.CategoryId));
        }
        List<ProductFeedback> feedbacks = await products
            .SelectMany(p => p.Feedbacks)
            .OrderByDescending(f => f.Created)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return feedbacks.Select(f => f.ToProductFeedbackDto());
    }

    public async Task<OperationResult<IEnumerable<ProductFeedbackDto>>> GetProductFeedbackAsync(int productId, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Feedbacks)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult<IEnumerable<ProductFeedbackDto>>(false,
                "Product with such an id does not exist.");
        }
        List<ProductFeedbackDto> productFeedbacksDto = product.Feedbacks
            .Select(f => f.ToProductFeedbackDto())
            .ToList();
        return new OperationResult<IEnumerable<ProductFeedbackDto>>(true, productFeedbacksDto);
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
            AuthorName = feedbackDto.AuthorName,
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

    public async Task<OperationResult> DeleteProductFeedbackAsync(int productId, int feedbackId,
        CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.Feedbacks)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult(false, $"No product with the id {productId} exists");
        }
        ProductFeedback? feedback = product.Feedbacks.Find(f => f.Id == feedbackId);
        if (feedback == null)
        {
            return new OperationResult(false, $"Product with the id {productId} does not contain " +
                $"the feedback with id {feedbackId}");
        }
        try
        {
            context.Remove(feedback);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while deleting feedback {@feedback}", feedback);
            return new OperationResult(false, "Feedback has not been deleted.");
        }
    }

    private async Task<OperationResult<ProductInstance>> GetProductInstanceAsync(int productId,
        int productInstanceId, CancellationToken cancellationToken)
    {
        Product? product = await context.Products
            .Include(p => p.ProductInstances)
                .ThenInclude(pi => pi.Images)
            .FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);
        if (product == null)
        {
            return new OperationResult<ProductInstance>(false, "The product does not exist");
        }
        ProductInstance? productInstance = product.ProductInstances
            .Find(pi => pi.Id == productInstanceId);
        if (productInstance == null)
        {
            return new OperationResult<ProductInstance>(false, "Product with id {productId} " +
                $"does not contain the product instance with id {productInstanceId}.");
        }
        return new OperationResult<ProductInstance>(true, productInstance);
    }
}
