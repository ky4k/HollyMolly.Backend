using HM.BLL.Models.Common;
using HM.BLL.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace HM.BLL.UnitTests.Services;

public class ImageServiceTests
{
    private readonly ILogger<ImageService> _logger;
    private readonly ImageService _imageService;
    public ImageServiceTests()
    {
        _logger = Substitute.For<ILogger<ImageService>>();
        _imageService = new ImageService(_logger);
    }
    ~ImageServiceTests()
    {
        string[] files = Directory.GetFiles("wwwroot/tests");
        Array.ForEach(files, File.Delete);
    }
    [Fact]
    public async Task UploadImageAsync_ShouldCreateFile()
    {
        IFormFile formFile = GetFormFile("image1.jpg");

        OperationResult<ImageDto> result = await _imageService
            .UploadImageAsync(formFile, "http://test.com", "tests", CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.True(File.Exists(result.Payload.FilePath));
    }
    [Fact]
    public async Task UploadImageAsync_ShouldReturnFalseResult_WhenFileHasInvalidContentType()
    {
        IFormFile formFile = GetFormFile("image1.jpg", "application/pdf");

        OperationResult<ImageDto> result = await _imageService
            .UploadImageAsync(formFile, "http://test.com", "tests", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UploadImageAsync_ShouldReturnFalseResult_WhenFileHasNotBeenSaved()
    {
        IFormFile formFile = GetFormFile("image1.jpg", "application/pdf");

        OperationResult<ImageDto> result = await _imageService
            .UploadImageAsync(formFile, "http://test.com", "tests:colon:not:supported", CancellationToken.None);

        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }
    [Fact]
    public async Task UploadImageAsync_ShouldReturnImagePathAndLink()
    {
        IFormFile formFile = GetFormFile("image1.jpg");

        OperationResult<ImageDto> result = await _imageService
            .UploadImageAsync(formFile, "http://test.com", "tests", CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Contains("tests", result.Payload.FilePath);
        Assert.Contains("http://test.com", result.Payload.Link);
    }
    [Fact]
    public async Task UploadImagesAsync_ShouldCreateFiles()
    {
        IFormFile[] formFiles =
        [
            GetFormFile("image1.jpg"),
            GetFormFile("image2.jpeg")
        ];

        OperationResult<List<ImageDto>> result = await _imageService
            .UploadImagesAsync(formFiles, "http://test.com", "tests", CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.True(File.Exists(result.Payload[0].FilePath));
        Assert.True(File.Exists(result.Payload[1].FilePath));
    }
    [Fact]
    public async Task UploadImagesAsync_ShouldReturnImagesPathsAndLinks()
    {
        IFormFile[] formFiles =
        [
            GetFormFile("image1.jpg"),
            GetFormFile("image2.jpeg")
        ];

        OperationResult<List<ImageDto>> result = await _imageService
            .UploadImagesAsync(formFiles, "http://test.com", "tests", CancellationToken.None);

        Assert.NotNull(result?.Payload);
        Assert.True(result.Succeeded);
        Assert.Equal(2, result.Payload.Count);
        Assert.Contains("tests", result.Payload[0].FilePath);
        Assert.Contains("http://test.com", result.Payload[0].Link);
    }

    [Fact]
    public async Task DeleteImage_ShouldDeleteImage()
    {
        IFormFile formFile = GetFormFile("image1.jpg");
        OperationResult<ImageDto> createdResult = await _imageService
            .UploadImageAsync(formFile, "http://test.com", "tests", CancellationToken.None);
        Assert.NotNull(createdResult?.Payload);
        Assert.True(createdResult.Succeeded);
        Assert.True(File.Exists(createdResult.Payload.FilePath));

        OperationResult result = _imageService.DeleteImage(createdResult.Payload.FilePath);

        Assert.NotNull(result);
        Assert.True(result.Succeeded);
        Assert.False(File.Exists(createdResult.Payload.FilePath));
    }
    [Fact]
    public async Task DeleteImage_ShouldReturnFalseResult_WhenErrorOccurredDuringDeleting()
    {
        IFormFile formFile = GetFormFile("image1.jpg");
        OperationResult<ImageDto> createdResult = await _imageService
            .UploadImageAsync(formFile, "http://test.com", "tests", CancellationToken.None);
        Assert.NotNull(createdResult?.Payload);
        Assert.True(createdResult.Succeeded);
        Assert.True(File.Exists(createdResult.Payload.FilePath));

        OperationResult result;
        using (StreamReader streamReader = new(createdResult.Payload.FilePath))
        {
            result = _imageService.DeleteImage(createdResult.Payload.FilePath);
        }
        Assert.NotNull(result);
        Assert.False(result.Succeeded);
    }

    private static FormFile GetFormFile(string fileName, string contentType = "image/jpeg")
    {
        var stream = new MemoryStream();
        var writer = new StreamWriter(stream);
        writer.Write("Fake image");
        writer.Flush();
        stream.Position = 0;

        return new(stream, 0, stream.Length, "test_image_1", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
