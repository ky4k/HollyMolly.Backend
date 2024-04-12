using HM.BLL.Models;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Http;

namespace HM.BLL.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(string? category, string? name, bool sortByPrice,
        bool sortByRating, bool sortAsc, CancellationToken cancellationToken);
    Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
    Task<OperationResult<ProductDto>> CreateProductAsync(ProductCreateUpdateDto productDto,
        CancellationToken cancellationToken);
    Task<OperationResult<ProductDto>> UpdateProductAsync(int productId, ProductCreateUpdateDto productDto,
        CancellationToken cancellationToken);
    Task<OperationResult> UploadProductImagesAsync(int productId, IFormFile[] images,
        string basePath, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductImageAsync(int productId, int imageId, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductAsync(int productId, CancellationToken cancellationToken);

    Task<IEnumerable<ProductFeedback>> GetAllProductsFeedbackAsync(string? category, CancellationToken cancellationToken);
    Task<OperationResult<IEnumerable<ProductFeedback>>> GetProductFeedbackAsync(
        int productId, CancellationToken cancellationToken);
    Task<OperationResult> AddFeedbackAsync(int productId, string? userId,
        ProductFeedbackCreateDto feedbackDto, CancellationToken cancellationToken);
}
