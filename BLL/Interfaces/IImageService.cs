using HM.BLL.Models;
using Microsoft.AspNetCore.Http;

namespace HM.BLL.Interfaces;

public interface IImageService
{
    Task<OperationResult<ImageDto>> UploadImageAsync(IFormFile image,
        string baseUrlPath, string savePath, CancellationToken cancellationToken);
    Task<OperationResult<List<ImageDto>>> UploadImagesAsync(IFormFile[] images,
        string baseUrlPath, string savePath, CancellationToken cancellationToken);
    OperationResult DeleteImage(string filePath);
}
