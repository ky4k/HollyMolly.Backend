﻿using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace HM.BLL.Services.NewPost;

public class NewPostCitiesService(
    IConfigurationHelper configurationHelper,
    IHttpClientFactory httpClientFactory,
    ILogger<NewPostCitiesService> logger
        ) : INewPostCitiesService
{
    private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
    private readonly ILogger<NewPostCitiesService> _logger = logger;
    private readonly string? _apiKey = configurationHelper.GetConfigurationValue("NewPost:APIKey");
    private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<IEnumerable<NewPostCities>> GetCitiesAsync(string? FindByString, string? Ref, string? Page, string? Limit, CancellationToken cancellationToken)
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
            if (apiResponse == null || !apiResponse.Data.Any())
            {
                _logger.LogError("Nova Poshta API response is null.");
                return Enumerable.Empty<NewPostCities>();
            }
            return apiResponse.Data;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError("HTTP Request error: {Message}", httpEx.Message);
            return Enumerable.Empty<NewPostCities>();
        }
        catch (TaskCanceledException taskCanceledEx)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Request was canceled.");
                return Enumerable.Empty<NewPostCities>();
            }
            _logger.LogError("Request timeout: {Message}", taskCanceledEx.Message);
            return Enumerable.Empty<NewPostCities>();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError("JSON Serialization/Deserialization error: {Message}", jsonEx.Message);
            return Enumerable.Empty<NewPostCities>();
        }
        catch (Exception ex)
        {
            _logger.LogError("An unexpected error occurred: {Message}", ex.Message);
            return Enumerable.Empty<NewPostCities>();
        }
    }

    public async Task<IEnumerable<NewPostWarehouse>> GetWarehousesAync(string? CityName,
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

            if (apiResponse == null || !apiResponse.Data.Any())
            {
                _logger.LogError("Nova Poshta API response is null.");
                return Enumerable.Empty<NewPostWarehouse>();
            }
            return apiResponse.Data;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "An error occurred while sending request to Nova Poshta API.");
            return Enumerable.Empty<NewPostWarehouse>();
        }
        catch (TaskCanceledException taskEx)
        {
            _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
            return Enumerable.Empty<NewPostWarehouse>();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
            return Enumerable.Empty<NewPostWarehouse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return Enumerable.Empty<NewPostWarehouse>();
        }
    }

    public async Task<bool> CheckIfCityIsValidAsync(string city, CancellationToken cancellationToken)
    {
        var result = await GetCitiesAsync(city, null, null, null, cancellationToken);
        return result.Any(c => c.Description.Equals(city, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken)
    {
        var result = await GetWarehousesAync(city, null, address, null, null, null, null, null, cancellationToken);
        return result.Any(warehouse => warehouse.Description.Contains(address, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<IEnumerable<NewPostStreets>> GetStreetsAync(string CityRef, string FindByString, string? Page,
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

            if (apiResponse == null || !apiResponse.Data.Any())
            {
                _logger.LogError("Nova Poshta API response is null.");
                return Enumerable.Empty<NewPostStreets>();
            }

            return apiResponse.Data;
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "An error occurred while sending a request to Nova Poshta API.");
            return Enumerable.Empty<NewPostStreets>();
        }
        catch (TaskCanceledException taskEx)
        {
            _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
            return Enumerable.Empty<NewPostStreets>();
        }
        catch (JsonException jsonEx)
        {
            _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
            return Enumerable.Empty<NewPostStreets>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred.");
            return Enumerable.Empty<NewPostStreets>();
        }
    }

    public async Task<OperationResult> UpadateCounterPartyAdressAsync(string CounterPartyRef, string AdressRef, string StreetRef, string? BuildingNumber, CancellationToken cancellationToken)
    {
        var requestPayload = new
        {
            apiKey = _apiKey,
            modelName = "AddressGeneral",
            calledMethod = "update",
            methodProperties = new
            {
                CounterpartyRef = CounterPartyRef,
                StreetRef = StreetRef,
                BuildingNumber = BuildingNumber,
                Ref = AdressRef
            }
        };

        var jsonRequestBody = JsonSerializer.Serialize(requestPayload, _jsonSerializerOptions);
        var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");

        try
        {
            _logger.LogInformation("Sending request to Nova Poshta API to update address with payload: {Payload}", jsonRequestBody);

            var response = await _httpClient.PostAsync("https://api.novaposhta.ua/v2.0/json/", content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
            _logger.LogInformation("Response from Nova Poshta API: {Response}", jsonResponse);

            if (jsonResponse.Contains("\"success\":true"))
            {
                return new OperationResult(true, "Address updated successfully.");
            }
            else
            {
                _logger.LogError("Failed to update address. Response did not indicate success.");
                return new OperationResult(false, "Failed to update address.");
            }
        }
        catch (HttpRequestException httpEx)
        {
            _logger.LogError(httpEx, "HTTP request error occurred while updating the address.");
            return new OperationResult(false, "HTTP request error: " + httpEx.Message);
        }
        catch (TaskCanceledException taskCanceledEx)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogWarning("Address update request was canceled.");
                return new OperationResult(false, "Request was canceled.");
            }
            _logger.LogError(taskCanceledEx, "Request timeout occurred while updating the address.");
            return new OperationResult(false, "Request timeout: " + taskCanceledEx.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred while updating the address.");
            return new OperationResult(false, "An unexpected error occurred: " + ex.Message);
        }
    }
}
