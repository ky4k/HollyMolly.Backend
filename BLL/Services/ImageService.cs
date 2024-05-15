using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace HM.BLL.Services;

public class ImageService(
    ILogger<ImageService> logger
    ) : IImageService
{
    public async Task<OperationResult<ImageDto>> UploadImageAsync(IFormFile image,
        string baseUrlPath, string savePath, CancellationToken cancellationToken)
    {
        OperationResult<List<ImageDto>> result = await UploadImagesAsync(
            [image], baseUrlPath, savePath, cancellationToken);
        return result.Succeeded && result.Payload!.Count > 0
            ? new OperationResult<ImageDto>(true, result.Payload[0])
            : new OperationResult<ImageDto>(false, result.Message!);
    }
    public async Task<OperationResult<List<ImageDto>>> UploadImagesAsync(IFormFile[] images,
        string baseUrlPath, string savePath, CancellationToken cancellationToken)
    {
        try
        {
            savePath = $"wwwroot/{savePath}";
            Directory.CreateDirectory(savePath);
            List<ImageDto> imagesDto = [];
            StringBuilder errorMessage = new();
            foreach (IFormFile image in images)
            {
                OperationResult validationResult = ValidateImage(image);
                if (!validationResult.Succeeded)
                {
                    errorMessage.Append(validationResult.Message);
                    continue;
                }
                string filePath = $"{savePath}/{GetUniqueFileName(image.FileName)}";
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream, cancellationToken);
                }
                string imageUrl = $"{baseUrlPath}/{filePath.Replace("wwwroot/", "")}";
                imagesDto.Add(new()
                {
                    FilePath = filePath,
                    Link = imageUrl
                });
            }
            return new OperationResult<List<ImageDto>>(true, errorMessage.ToString(), imagesDto);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image was not created.");
            return new OperationResult<List<ImageDto>>(false, "An error occurred while saving image.");
        }

    }

    public OperationResult DeleteImage(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return new OperationResult(true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Image {Image} was not deleted.", filePath);
            return new OperationResult(false, ex.Message);
        }
    }

    private static string GetUniqueFileName(string fileName)
    {
        int index = fileName.ToString().LastIndexOf('.');
        string fileNameWithoutExtension = fileName.ToString()[..index];
        return $"{fileNameWithoutExtension}-{Guid.NewGuid().ToString()[..4]}.jpg";
    }

    private static OperationResult ValidateImage(IFormFile image)
    {
        return image.ContentType == "image/jpeg" &&
            (image.FileName.EndsWith(".jpg") || image.FileName.EndsWith(".jpeg"))
                ? new OperationResult(true)
                : new OperationResult(false, $"Invalid file format of {image.FileName}");
    }
}
