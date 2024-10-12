using DocumentFormat.OpenXml.InkML;
using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;
using HM.DAL.Data;
using HM.DAL.Entities.NewPost;
using Microsoft.EntityFrameworkCore;
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
using System.Linq;

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

        public async Task<NewPostCounterAgentDto> GetCounterpartyAsync(CustomerDto customerDto, CancellationToken cancellationToken)
        {
            try
            {
                var existingCounterAgent = await _dbContext.NewPostCounterAgents
                .AsNoTracking()
                .Include(ca => ca.ContactPersons)
                .FirstOrDefaultAsync(ca => ca.FirstName == customerDto.FirstName &&
                                           ca.LastName == customerDto.LastName &&
                                           ca.ContactPersons.Any(cp => 
                                               cp.Phones.EndsWith(customerDto.PhoneNumber.Length==10 ? customerDto.PhoneNumber: customerDto.PhoneNumber.Substring(3)) &&
                                               cp.Email == customerDto.Email),
                                           cancellationToken);

                if (existingCounterAgent == null)
                {
                    _logger.LogWarning("Counteragent not found for customer: {FirstName} {LastName}.", customerDto.FirstName, customerDto.LastName);
                    return null;
                }
                var counterAgentDto = existingCounterAgent.ToNewPostCounterAgentDto();
                return counterAgentDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while retrieving the counteragent.");
                throw;
            }
        }

        public async Task<OperationResult<IEnumerable<NewPostCounterAgentDto>>> CreateCounterpartyAsync(CustomerDto customerDto, CancellationToken cancellationToken)
        {
            var existingCounterAgentDto = await GetCounterpartyAsync(customerDto, cancellationToken);

            if (existingCounterAgentDto != null)
            {
                return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(true, string.Empty, new List<NewPostCounterAgentDto> { existingCounterAgentDto });
            }
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

                var apiResponse = JsonSerializer.Deserialize<NewPostResponseData<NewPostCounterAgentDto>>(jsonResponse, _jsonSerializerOptions);

                if (apiResponse == null)
                {
                    _logger.LogError("Nova Poshta API response is null.");
                    return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(false, "Response is null.", new List<NewPostCounterAgentDto>());
                }

                foreach (var counterAgentDto in apiResponse.Data)
                {
                    var contactPersons = await GetContactPersonsAsync(counterAgentDto.Ref, "1", cancellationToken);
                    var filteredContactPersons = contactPersons.Where(contactPerson =>
                        string.Equals(contactPerson.FirstName, counterAgentDto.FirstName, StringComparison.OrdinalIgnoreCase) &&
                        string.Equals(contactPerson.LastName, counterAgentDto.LastName, StringComparison.OrdinalIgnoreCase) &&
                        (string.IsNullOrEmpty(counterAgentDto.MiddleName) || string.Equals(contactPerson.MiddleName, counterAgentDto.MiddleName, StringComparison.OrdinalIgnoreCase)))
                    .Select(contactPersonDto =>
                    {
                        return new NewPostContactPerson
                        {
                            Ref = contactPersonDto.Ref, 
                            FirstName = contactPersonDto.FirstName,
                            LastName = contactPersonDto.LastName,
                            MiddleName = contactPersonDto.MiddleName,
                            Description = contactPersonDto.Description,
                            Phones = contactPersonDto.Phones.StartsWith("+") ? contactPersonDto.Phones : $"+{contactPersonDto.Phones}",
                            Email = contactPersonDto.Email
                        };
                    }).ToList();

                    var counterAgent = counterAgentDto.ToNewPostCounterAgent();
                    counterAgent.ContactPersons = filteredContactPersons;

                    _dbContext.NewPostCounterAgents.Add(counterAgent);
                }
                await _dbContext.SaveChangesAsync(cancellationToken);

                var savedCounterAgentDto = await GetCounterpartyAsync(customerDto, cancellationToken);
                return new OperationResult<IEnumerable<NewPostCounterAgentDto>>(true, string.Empty, new List<NewPostCounterAgentDto> { savedCounterAgentDto });
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

        public async Task<IEnumerable<NewPostCounterAgents>> GetSendersListAsync(string Page, CancellationToken cancellationToken)
        {
            var requestBody = new
            {
                apiKey = _apiKey,
                modelName = "CounterpartyGeneral",
                calledMethod = "getCounterparties",
                methodProperties = new
                {
                    CounterpartyProperty = "Sender",
                    Page = Page
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

                var apiResponse = JsonSerializer.Deserialize<NewPostResponseData<NewPostCounterAgents>>(jsonResponse, _jsonSerializerOptions);

                if (apiResponse == null)
                {
                    _logger.LogError("Failed to retrieve data.");
                    return Enumerable.Empty<NewPostCounterAgents>();
                }
                return apiResponse.Data;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "An error occurred while sending a request to Nova Poshta API.");
                return Enumerable.Empty<NewPostCounterAgents>();
            }
            catch (TaskCanceledException taskEx)
            {
                _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
                return Enumerable.Empty<NewPostCounterAgents>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
                return Enumerable.Empty<NewPostCounterAgents>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                return Enumerable.Empty<NewPostCounterAgents>();
            }
        }

        public async Task<IEnumerable<NewPostCounterAgentAdress>> GetCounterpartyAdressAsync(string counterPartyRef,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(counterPartyRef))
            {
                _logger.LogWarning("CounterPartyRef is null or empty.");
                return Enumerable.Empty<NewPostCounterAgentAdress>();
            }

            var requestBody = new
            {
                apiKey = _apiKey,
                modelName = "CounterpartyGeneral",
                calledMethod = "getCounterpartyAddresses",
                methodProperties = new
                {
                    Ref = counterPartyRef,
                    CounterpartyProperty = string.Empty
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

                var apiResponse = JsonSerializer.Deserialize<NewPostResponseData<NewPostCounterAgentAdress>>(jsonResponse, _jsonSerializerOptions);

                if (apiResponse == null || apiResponse.Data == null || !apiResponse.Data.Any())
                {
                    _logger.LogError("Nova Poshta API returned an empty or null data.");
                    return Enumerable.Empty<NewPostCounterAgentAdress>();
                }

                return apiResponse.Data;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "An error occurred while sending a request to Nova Poshta API.");
                return Enumerable.Empty<NewPostCounterAgentAdress>();
            }
            catch (TaskCanceledException taskEx)
            {
                _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
                return Enumerable.Empty<NewPostCounterAgentAdress>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
                return Enumerable.Empty<NewPostCounterAgentAdress>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                return Enumerable.Empty<NewPostCounterAgentAdress>();
            }
        }
        public async Task<IEnumerable<NewPostContactPersonDto>> GetContactPersonsAsync(string counterPartyRef, string Page, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(counterPartyRef))
            {
                _logger.LogWarning("CounterPartyRef is null or empty.");
                return Enumerable.Empty<NewPostContactPersonDto>();
            }
            var requestBody = new
            {
                apiKey = _apiKey,
                modelName = "CounterpartyGeneral",
                calledMethod = "getCounterpartyContactPersons",
                methodProperties = new
                {
                    Ref = counterPartyRef,
                    Page = Page
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

                var apiResponse = JsonSerializer.Deserialize<NewPostResponseData<NewPostContactPersonDto>>(jsonResponse, _jsonSerializerOptions);

                if (apiResponse == null || apiResponse.Data == null || !apiResponse.Data.Any())
                {
                    _logger.LogError("Nova Poshta API returned an empty or null data.");
                    return Enumerable.Empty<NewPostContactPersonDto>();
                }

                return apiResponse.Data;
            }
            catch (HttpRequestException httpEx)
            {
                _logger.LogError(httpEx, "An error occurred while sending a request to Nova Poshta API.");
                return Enumerable.Empty<NewPostContactPersonDto>();
            }
            catch (TaskCanceledException taskEx)
            {
                _logger.LogError(taskEx, "The request to Nova Poshta API was canceled.");
                return Enumerable.Empty<NewPostContactPersonDto>();
            }
            catch (JsonException jsonEx)
            {
                _logger.LogError(jsonEx, "Error deserializing the response from Nova Poshta API.");
                return Enumerable.Empty<NewPostContactPersonDto>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unexpected error occurred.");
                return Enumerable.Empty<NewPostContactPersonDto>();
            }
        }
    }
}
