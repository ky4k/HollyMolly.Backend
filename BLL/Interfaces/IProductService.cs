﻿using HM.BLL.Models;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Http;

namespace HM.BLL.Interfaces;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetProductsAsync(int? categoryGroupId, int? categoryId, string? name,
        bool sortByPrice, bool sortByRating, bool sortAsc, CancellationToken cancellationToken);
    Task<ProductDto?> GetProductByIdAsync(int productId, CancellationToken cancellationToken);
    Task<OperationResult<ProductDto>> CreateProductAsync(ProductCreateDto productDto,
        CancellationToken cancellationToken);
    Task<OperationResult<ProductDto>> UpdateProductAsync(int productId, ProductUpdateDto productDto,
        CancellationToken cancellationToken);
    Task<OperationResult<ProductInstanceDto>> UpdateProductInstanceAsync(int productId, int productInstanceId,
        ProductInstanceCreateDto productInstanceDto, CancellationToken cancellationToken);
    Task<OperationResult<ProductInstanceDto>> UploadProductImagesAsync(int productId, int productInstanceId,
        IFormFile[] images, string baseUrlPath, CancellationToken cancellationToken);
    Task<OperationResult> RearrangeProductImagesAsync(int productId, int productInstanceId,
        List<ProductImageRearrangeDto> imageRearrangesDto, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductImageAsync(int productId, int productInstanceId,
        int imageId, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductAsync(int productId, CancellationToken cancellationToken);

    Task<IEnumerable<ProductFeedbackDto>> GetAllProductsFeedbackAsync(string? category, CancellationToken cancellationToken);
    Task<OperationResult<IEnumerable<ProductFeedbackDto>>> GetProductFeedbackAsync(
        int productId, CancellationToken cancellationToken);
    Task<OperationResult> AddFeedbackAsync(int productId, string? userId,
        ProductFeedbackCreateDto feedbackDto, CancellationToken cancellationToken);
    Task<OperationResult> DeleteProductFeedbackAsync(int productId, int feedbackId, CancellationToken cancellationToken);
}
