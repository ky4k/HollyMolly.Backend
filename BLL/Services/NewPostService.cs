using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Text;
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
    private readonly string? _apiKey = configurationHelper.GetConfigurationValue("NewPost:APIKey");
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

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
            var apiResponse = JsonSerializer.Deserialize<NewPostResponse<NewPostWarehouse>>(jsonResponse, _jsonSerializerOptions);

            if (apiResponse == null)
            {
                _logger.LogError("Nova Poshta API response is null.");
                return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Response is null.", null);
            }
            if (!apiResponse.Success)
            {
                _logger.LogError("Nova Poshta API response indicates failure.");
                var errorMessage = apiResponse.Errors.Count > 0 ? string.Join(", ", apiResponse.Errors) : "Unknown error";
                return new OperationResult<IEnumerable<NewPostWarehouse>>(false, errorMessage, null);
            }
            return new OperationResult<IEnumerable<NewPostWarehouse>>(true, string.Empty, apiResponse.Data);
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "An error occurred while sending request to Nova Poshta API.");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Request error.", null);
        }
        catch (TaskCanceledException taskEx)
        {
            _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Request was canceled.", null);
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "Error parsing response.", null);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return new OperationResult<IEnumerable<NewPostWarehouse>>(false, "An unexpected error occurred.", null);
        }
    }
}
