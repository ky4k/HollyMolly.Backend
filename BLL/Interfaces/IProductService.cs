using HM.BLL.Models.Common;
using HM.BLL.Models.Products;
using Microsoft.AspNetCore.Http;

namespace HM.BLL.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(int? categoryGroupId, int? categoryId, string? name,
        bool onlyNewCollection, bool sortByPrice, bool sortByRating, bool sortAsc, CancellationToken cancellationToken);
    Task<IEnumerable<ProductDto>> GetRecommendedProductsAsync(int number, CancellationToken cancellationToken);
    Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
    Task<OperationResult<ProductDto>> CreateProductAsync(ProductCreateDto productDto, CancellationToken cancellationToken);
    Task<OperationResult<ProductDto>> UpdateProductAsync(
        int productId, ProductUpdateDto productDto, CancellationToken cancellationToken);
    Task<OperationResult<ProductInstanceDto>> AddProductInstanceToProductAsync(
        int productId, ProductInstanceCreateDto productInstanceDto, CancellationToken cancellationToken);
    Task<OperationResult<ProductInstanceDto>> UpdateProductInstanceAsync(
        int productId, int productInstanceId, ProductInstanceCreateDto productInstanceDto, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductInstanceAsync(int productId, int productInstanceId, CancellationToken cancellationToken);
    Task<OperationResult<ProductInstanceDto>> UploadProductImagesAsync(int productId,
        int productInstanceId, IFormFile[] images, string baseUrlPath, CancellationToken cancellationToken);
    Task<OperationResult> RearrangeProductImagesAsync(int productId, int productInstanceId,
        List<ProductImageRearrangeDto> imageRearrangesDto, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductImageAsync(int productId, int productInstanceId,
        int imageId, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductAsync(int productId, CancellationToken cancellationToken);

    Task<IEnumerable<ProductFeedbackDto>> GetAllProductsFeedbackAsync(
        int? categoryGroupId, int? categoryId, CancellationToken cancellationToken);
    Task<OperationResult<IEnumerable<ProductFeedbackDto>>> GetProductFeedbackAsync(
        int productId, CancellationToken cancellationToken);
    Task<OperationResult> AddFeedbackAsync(int productId, string? userId,
        ProductFeedbackCreateDto feedbackDto, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductFeedbackAsync(int productId, int feedbackId, CancellationToken cancellationToken);
}
