using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HM.BLL.Services;

public partial class NewPostService(
    IConfigurationHelper configurationHelper,
    IHttpClientFactory httpClientFactory,
    ILogger<NewPostService> logger
        ) : INewPostService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly ILogger<NewPostService> _logger = logger;
    private readonly string? _baseUrl = configurationHelper.GetConfigurationValue("NewPost:BaseUrl");
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<OperationResult<IEnumerable<NewPostCity>>> GetCitiesAsync(
        string? name, CancellationToken cancellationToken)
    {
        name ??= "";
        string city = RegionPattern().Replace(name, "").Trim();
        string uri = _baseUrl + $"/cities?q={Uri.EscapeDataString(city)}";
        try
        {
            HttpResponseMessage? response = await _httpClient.GetAsync(uri, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (content.Length < 3)
            {
                return new OperationResult<IEnumerable<NewPostCity>>(true, []);
            }
            NewPostResponse<NewPostCity>? deserialized = JsonSerializer
                .Deserialize<NewPostResponse<NewPostCity>>(content, _jsonSerializerOptions);
            IEnumerable<NewPostCity> cities = deserialized?.Results?.Where(c => c.Id.Contains(name!, StringComparison.CurrentCultureIgnoreCase)) ?? [];
            return new OperationResult<IEnumerable<NewPostCity>>(true, cities);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred when getting cities from New Post");
            return new OperationResult<IEnumerable<NewPostCity>>(false, "Server cannot get cities!");
        }
    }

    public async Task<OperationResult<IEnumerable<NewPostWarehouse>>> GetWarehousesAsync(
        string? warehouse, string koatuu, int page, CancellationToken cancellationToken)
    {
        string uri = _baseUrl + $"/warehouse?warehouse={warehouse}&city={koatuu}&page={page}";
        try
        {
            HttpResponseMessage? response = await _httpClient.GetAsync(uri, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (content.Length < 3)
            {
                return new OperationResult<IEnumerable<NewPostWarehouse>>(true, []);
            }
            NewPostResponse<NewPostWarehouse>? cities = JsonSerializer
                .Deserialize<NewPostResponse<NewPostWarehouse>>(content, _jsonSerializerOptions);
            return new OperationResult<IEnumerable<NewPostWarehouse>>(true, cities!.Results ?? []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred when getting cities from New Post");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Server cannot get warehouses!");
        }
    }
    public async Task<bool> CheckIfCityIsValidAsync(string city, CancellationToken cancellationToken)
    {
        OperationResult<IEnumerable<NewPostCity>> resultCity = await GetCitiesAsync(city, cancellationToken);
        return resultCity.Payload?.FirstOrDefault(c => c.Id == city) != null;
    }
    public async Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken)
    {
        OperationResult<IEnumerable<NewPostCity>> resultCity = await GetCitiesAsync(city, cancellationToken);
        NewPostCity? newPostCity = resultCity.Payload?.FirstOrDefault(c => c.Id == city);
        if (newPostCity == null)
        {
            return false;
        }
        address = address.Trim();
        OperationResult<IEnumerable<NewPostWarehouse>> resultWarehouse =
            await GetWarehousesAsync(address, newPostCity.Koatuu, 1, cancellationToken);

        return resultWarehouse.Payload?.Any(w => w.Id == address) ?? false;
    }

    [GeneratedRegex(@"\(.*\)|\(.*$| село| селище| смт| місто")]
    private static partial Regex RegionPattern();
}
