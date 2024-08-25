using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace HM.BLL.Services;

public class NewPostService(
    IConfigurationHelper configurationHelper,
    IHttpClientFactory httpClientFactory,
    ILogger<NewPostService> logger
        ) : INewPostCityesService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly ILogger<NewPostService> _logger = logger;
    private readonly string? _apiKey = configurationHelper.GetConfigurationValue("NewPost:APIKey");
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<OperationResult<IEnumerable<NewPostCities>>> GetCitiesAsync(string? FindByString, string? Ref, string? Page, string? Limit, CancellationToken cancellationToken)
    {
        var request = new
        {
            apiKey = _apiKey,
            modelName = "AddressGeneral",
            calledMethod = "getCities",
            methodProperties = new
            {
                FindByString = FindByString ?? string.Empty,
                Ref = Ref ?? string.Empty,
                Page = Page ?? "1",
                Limit = Limit ?? "50",
            }
        };
        var jsonRequestBody = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync("https://api.novaposhta.ua/v2.0/json/", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Response from Nova Poshta API: {Response}", jsonResponse);
            var apiResponse = JsonSerializer.Deserialize<NewPostResponseData<NewPostCities>>(jsonResponse, _jsonSerializerOptions);
            if (apiResponse == null)
            {
                _logger.LogError("Nova Poshta API response is null.");
                return new OperationResult<IEnumerable<NewPostCities>>(false, "Response is null.", []);
            }
            return new OperationResult<IEnumerable<NewPostCities>>(true, string.Empty, apiResponse.Data);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError("HTTP Request error: {Message}", httpEx.Message);
            return new OperationResult<IEnumerable<NewPostCities>>(false, "HTTP Request error: " + httpEx.Message, []);
        }
        catch (TaskCanceledException taskCanceledEx)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Request was canceled.");
                return new OperationResult<IEnumerable<NewPostCities>>(false, "Request was canceled.", []);
            }
            _logger.LogError("Request timeout: {Message}", taskCanceledEx.Message);
            return new OperationResult<IEnumerable<NewPostCities>>(false, "Request timeout: " + taskCanceledEx.Message, []);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError("JSON Serialization/Deserialization error: {Message}", jsonEx.Message);
            return new OperationResult<IEnumerable<NewPostCities>>(false, "JSON error: " + jsonEx.Message, []);
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
            return new OperationResult<IEnumerable<NewPostCities>>(false, "An unexpected error occurred: " + ex.Message, []);
        }
    }

    public async Task<OperationResult<IEnumerable<NewPostWarehouse>>> GetWarehousesAync(string? CityName,
        string? WarehouseId, string? FindByString, string? CityRef, string? Page, string? Limit,
        string? Language, string? TypeOfWarehouseRef, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request parameters: CityName={CityName}, WarehouseId={WarehouseId}, FindByString={FindByString}, CityRef={CityRef}, Page={Page}, Limit={Limit}, Language={Language}, TypeOfWarehouseRef={TypeOfWarehouseRef}",
            CityName, WarehouseId, FindByString, CityRef, Page, Limit, Language, TypeOfWarehouseRef);
        var request = new
        {
            apiKey = _apiKey,
            modelName = "AddressGeneral",
            calledMethod = "getWarehouses",
            methodProperties = new
            {
                FindByString = FindByString ?? string.Empty,
                CityName = CityName ?? string.Empty,
                CityRef = CityRef ?? string.Empty,
                Page = Page ?? "1",
                Limit = Limit ?? "50",
                Language = Language ?? "UA",
                TypeOfWarehouseRef = TypeOfWarehouseRef ?? string.Empty,
                WarehouseId = WarehouseId ?? string.Empty
            }
        };
        var jsonRequestBody = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
        try
        {
            var response = await _httpClient.PostAsync("https://api.novaposhta.ua/v2.0/json/", content, cancellationToken);
            response.EnsureSuccessStatusCode();
            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Response from Nova Poshta API: {Response}", jsonResponse);
            var apiResponse = JsonSerializer.Deserialize<NewPostResponseData<NewPostWarehouse>>(jsonResponse, _jsonSerializerOptions);

            if (apiResponse == null)
            {
                _logger.LogError("Nova Poshta API response is null.");
                return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Response is null.", []);
            }
            return new OperationResult<IEnumerable<NewPostWarehouse>>(true, string.Empty, apiResponse.Data);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "An error occurred while sending request to Nova Poshta API.");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Request error.", []);
        }
        catch (TaskCanceledException taskEx)
        {
            _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Request was canceled.", []);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Error parsing response.", []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "An unexpected error occurred.", []);
        }
    }

    public async Task<bool> CheckIfCityIsValidAsync(string city, CancellationToken cancellationToken)
    {
        var result = await GetCitiesAsync(city, null, null, null, cancellationToken);
        return result.Payload?.Any(c => c.Description.Equals(city, StringComparison.OrdinalIgnoreCase)) ?? false;
    }

    public async Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken)
    {
        var result = await GetWarehousesAync(city, null, address, null, null, null, null, null, cancellationToken);
        return result.Payload?.Any(warehouse => warehouse.Description.Contains(address, StringComparison.OrdinalIgnoreCase)) ?? false;
    }

    public async Task<OperationResult<IEnumerable<NewPostStreets>>> GetStreetsAync(string CityRef, string FindByString, string? Page,
        string? Limit, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Request parameters: CityRef={CityRef}, FindByString={FindByString}, Page={Page}, Limit={Limit}",
       CityRef, FindByString, Page, Limit);

        var request = new
        {
            apiKey = _apiKey,
            modelName = "AddressGeneral",
            calledMethod = "getStreet",
            methodProperties = new
            {
                CityRef = CityRef,
                FindByString = FindByString ?? string.Empty,
                Page = Page ?? "1",
                Limit = Limit ?? "50"
            }
        };

        var jsonRequestBody = JsonSerializer.Serialize(request, _jsonSerializerOptions);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        try
        {
            var response = await _httpClient.PostAsync("https://api.novaposhta.ua/v2.0/json/", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Response from Nova Poshta API: {Response}", jsonResponse);
            var apiResponse = JsonSerializer.Deserialize<NewPostResponseData<NewPostStreets>>(jsonResponse, _jsonSerializerOptions);

            if (apiResponse == null)
            {
                _logger.LogError("Nova Poshta API response is null.");
                return new OperationResult<IEnumerable<NewPostStreets>>(false, "Response is null.", Enumerable.Empty<NewPostStreets>());
            }

            return new OperationResult<IEnumerable<NewPostStreets>>(true, string.Empty, apiResponse.Data);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "An error occurred while sending a request to Nova Poshta API.");
            return new OperationResult<IEnumerable<NewPostStreets>>(false, "Request error.", Enumerable.Empty<NewPostStreets>());
        }
        catch (TaskCanceledException taskEx)
        {
            _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
            return new OperationResult<IEnumerable<NewPostStreets>>(false, "Request was canceled.", Enumerable.Empty<NewPostStreets>());
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
            return new OperationResult<IEnumerable<NewPostStreets>>(false, "Error parsing response.", Enumerable.Empty<NewPostStreets>());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return new OperationResult<IEnumerable<NewPostStreets>>(false, "An unexpected error occurred.", Enumerable.Empty<NewPostStreets>());
        }
    }
}
