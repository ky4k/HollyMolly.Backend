using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
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
        var request = new
        {
            apiKey = _apiKey,
            modelName = "AddressGeneral",
            calledMethod = "getWarehouses",
            methodProperties = new
            {
                FindByString,
                CityName,
                CityRef,
                Page,
                Limit,
                Language,
                TypeOfWarehouseRef,
                WarehouseId
            }
        };
        try
        {
            var response = await _httpClient.PostAsJsonAsync("https://api.novaposhta.ua/v2.0/json/", request, cancellationToken);
            response.EnsureSuccessStatusCode();
            return null;
        }
        catch (Exception ex)
        {
            return null;
        }
    }
}
