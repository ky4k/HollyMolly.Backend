using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HM.BLL.Services;

public partial class NewPostService(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<NewPostService> logger
        ) : INewPostService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly ILogger<NewPostService> _logger = logger;
    private readonly string? _baseUrl = Environment.GetEnvironmentVariable("NewPost:BaseUrl")
            ?? configuration["NewPost:BaseUrl"];
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<OperationResult<IEnumerable<NewPostCity>>> GetCitiesAsync(
        string? name, int page, CancellationToken cancellationToken)
    {
        string uri = _baseUrl + $"/cities?q={Uri.EscapeDataString(name ?? "")}&page={page}";

        try
        {
            HttpResponseMessage? response = await _httpClient.GetAsync(uri, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (content.Length < 3)
            {
                return new OperationResult<IEnumerable<NewPostCity>>(true, Array.Empty<NewPostCity>());
            }
            using var stream = response.Content.ReadAsStream(cancellationToken);
            NewPostResponse<NewPostCity>? cities = JsonSerializer
                .Deserialize<NewPostResponse<NewPostCity>>(stream, _jsonSerializerOptions);
            return new OperationResult<IEnumerable<NewPostCity>>(true, cities!.Results ?? []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred when getting cities from New Post");
            return new OperationResult<IEnumerable<NewPostCity>>(false, "Server cannot get cities!");
        }
    }

    public async Task<OperationResult<IEnumerable<NewPostWarehouse>>> GetWarehousesAsync(
        string? koatuu, int page, CancellationToken cancellationToken)
    {
        string uri = _baseUrl + $"/warehouse?city={koatuu}&page={page}";

        try
        {
            HttpResponseMessage? response = await _httpClient.GetAsync(uri, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            if (content.Length < 3)
            {
                return new OperationResult<IEnumerable<NewPostWarehouse>>(true, Array.Empty<NewPostWarehouse>());
            }
            using var stream = response.Content.ReadAsStream(cancellationToken);
            NewPostResponse<NewPostWarehouse>? cities = JsonSerializer
                .Deserialize<NewPostResponse<NewPostWarehouse>>(stream, _jsonSerializerOptions);
            return new OperationResult<IEnumerable<NewPostWarehouse>>(true, cities!.Results ?? []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred when getting cities from New Post");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Server cannot get warehouses!");
        }
    }

    public async Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken)
    {
        string toSearch = city;
        toSearch = RegionPattern().Replace(toSearch, "").Replace(" село", "").Replace(" селище", "").Trim();
        OperationResult<IEnumerable<NewPostCity>> resultCity = await GetCitiesAsync(toSearch, 1, cancellationToken);
        if (!resultCity.Succeeded)
        {
            return false;
        }
        NewPostCity? newPostCity = resultCity.Payload!.FirstOrDefault(c => c.Id == city);
        if (newPostCity == null)
        {
            return false;
        }
        address = address.Trim();

        int page = 1;
        bool found = false;
        while (!found && page < 100)
        {
            OperationResult<IEnumerable<NewPostWarehouse>> resultWarehouse =
                await GetWarehousesAsync(newPostCity.Koatuu, page, cancellationToken);
            if (!resultWarehouse.Succeeded)
            {
                break;
            }
            found = resultWarehouse.Payload!.Any(w => w.Id == address);
            page++;
        }
        return found;
    }

    [GeneratedRegex(@"\(.*\)")]
    private static partial Regex RegionPattern();
}
