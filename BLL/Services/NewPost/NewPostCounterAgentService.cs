using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;
using HM.DAL.Data;
using HM.DAL.Entities.NewPost;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace HM.BLL.Services.NewPost
{
    public class NewPostCounterAgentService(
    HmDbContext _dbContext,
    IConfigurationHelper configurationHelper,
    IHttpClientFactory httpClientFactory,
    ILogger<NewPostCounterAgentService> logger) : INewPostCounerAgentService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly ILogger<NewPostCounterAgentService> _logger = logger;
        private readonly string? _apiKey = configurationHelper.GetConfigurationValue("NewPost:APIKey");
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        public async Task<OperationResult<IEnumerable<NewPostCounterAgentDto>>> CreateCounterpartyAsync(CustomerDto customerDto, CancellationToken cancellationToken)
        {
            var requestBody = new
            {
                apiKey = _apiKey,
                modelName = "CounterpartyGeneral",
                calledMethod = "save",
                methodProperties = new
                {
                    FirstName = customerDto.FirstName,
                    MiddleName = string.Empty,
                    LastName = customerDto.LastName,
                    Phone = customerDto.PhoneNumber,
                    Email = customerDto.Email,
                    CounterpartyType = "PrivatePerson",
                    CounterpartyProperty = "Recipient"
                }
            };
            var jsonRequestBody = JsonSerializer.Serialize(requestBody, _jsonSerializerOptions);
            var content = new StringContent(jsonRequestBody, Encoding.UTF8, "application/json");
            try
            {
                var response = await _httpClient.PostAsync("https://api.novaposhta.ua/v2.0/json/", content, cancellationToken);
                response.EnsureSuccessStatusCode();
                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogInformation("Response from Nova Poshta API: {Response}", jsonResponse);

                var apiResponse = JsonSerializer.Deserialize<NewPostResponse<NewPostCounterAgent>>(jsonResponse, _jsonSerializerOptions);

                if (apiResponse == null)
                {
                    _logger.LogError("Nova Poshta API response is null.");
                    return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(false, "Response is null.", new List<NewPostCounterAgentDto>());
                }

                if (!apiResponse.Success)
                {
                    _logger.LogError("Nova Poshta API response indicates failure.");
                    var errorMessage = apiResponse.Errors.Count > 0 ? string.Join(", ", apiResponse.Errors) : "Unknown error";
                    return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(false, errorMessage, new List<NewPostCounterAgentDto>());
                }
                var counterAgents = apiResponse.Data.Select(agent => agent.ToNewPostCounterAgentDto()).ToList();

                foreach (var counterAgentDto in counterAgents)
                {
                    var counterAgentEntity = counterAgentDto.ToNewPostCounterAgent();
                    _dbContext.NewPostCounterAgents.Add(counterAgentEntity);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);

                return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(true, string.Empty, counterAgents);
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "An error occurred while sending a request to Nova Poshta API.");
                return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(false, "Request error.", Enumerable.Empty<NewPostCounterAgentDto>());
            }
            catch (TaskCanceledException taskEx)
            {
                _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
                return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(false, "Request was canceled.", Enumerable.Empty<NewPostCounterAgentDto>());
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
                return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(false, "Error parsing response.", Enumerable.Empty<NewPostCounterAgentDto>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(false, "An unexpected error occurred.", Enumerable.Empty<NewPostCounterAgentDto>());
            }
        }
    }
}
