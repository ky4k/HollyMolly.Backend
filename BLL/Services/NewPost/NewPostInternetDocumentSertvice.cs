using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.DAL.Data;
using HM.DAL.Entities.NewPost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Text.Json;

namespace HM.BLL.Services.NewPost
{
    public class NewPostInternetDocumentService(
    HmDbContext _dbContext,
    IConfigurationHelper configurationHelper,
    IHttpClientFactory httpClientFactory,
    ILogger<NewPostCounterAgentService> logger,
    IOrderService orderService,
    INewPostCounerAgentService newPostCounerAgentService,
    INewPostCitiesService newPostCityesService) : INewPostInternetDocumentService
    {
        private readonly HttpClient _httpClient = httpClientFactory.CreateClient();
        private readonly ILogger<NewPostCounterAgentService> _logger = logger;
        private readonly string? _apiKey = configurationHelper.GetConfigurationValue("NewPost:APIKey");
        private readonly JsonSerializerOptions _jsonSerializerOptions = new() { PropertyNameCaseInsensitive = true };
        private string ExtractStreetName(string address)
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be null or empty.", nameof(address));
            }

            var parts = address.Split(',');

            return parts[1].Trim();
        }
        private string ExtractBuildNumber(string address) 
        {
            if (string.IsNullOrWhiteSpace(address))
            {
                throw new ArgumentException("Address cannot be null or empty.");
            }
            var parts = address.Split(',').Select(p => p.Trim()).ToArray();
            var buildingNumber = parts.Last();

            return buildingNumber;
        }
        private OperationResult<NewPostInternetDocumentDto> CheckParameters(int orderid, string SenderWarehouseIndex, string senderRef,
            string PayerType, string PaymentMethod, DateTimeOffset DateOfSend, float weight, string serviceType, string SeatsAmount,
            string description, float costOfEstimate, float costOfGood)
        {
            if (string.IsNullOrEmpty(SenderWarehouseIndex))
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid SenderWarehouseIndex. Empty string.");
            }

            if (string.IsNullOrEmpty(senderRef))
            {
                return new OperationResult<NewPostInternetDocumentDto> (false, "Invalid senderRef. Empty string.");
            }

            var validPayerTypes = new[] { "Sender", "Recipient", "ThirdPerson" };
            if (!validPayerTypes.Contains(PayerType))
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid PayerType. Must be 'Sender', 'Recipient' or 'ThirdPerson'.");
            }

            var validPaymentMethods = new[] { "Cash", "NonCash" };
            if (!validPaymentMethods.Contains(PaymentMethod))
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid PaymentMethod. Must be 'Cash' or 'NonCash'.");
            }

            if (DateOfSend < DateTimeOffset.Now.Date)
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid DateOfSend. Cannot be earlier than today.");
            }

            if (weight < 0.1)
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid weight. Must be at least 0.1 kg.");
            }

            var validServiceTypes = new[] { "DoorsDoors", "DoorsWarehouse", "WarehouseWarehouse", "WarehouseDoors" };
            if (!validServiceTypes.Contains(serviceType))
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid serviceType. Must be one of 'DoorsDoors', 'DoorsWarehouse', 'WarehouseWarehouse', 'WarehouseDoors'.");
            }

            if (!int.TryParse(SeatsAmount, out int seats) || seats < 1)
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid SeatsAmount. Must be an integer greater than 0.");
            }

            if (string.IsNullOrEmpty(description))
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid description. It cannot be empty.");
            }

            if (costOfEstimate < 0)
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid costOfEstimate. Must be more then 0");
            }

            if (costOfGood <= 0)
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Invalid costOfGood. Must be greater than 0.");
            }
            return new OperationResult<NewPostInternetDocumentDto>(true, "Parameters are valid.");
        }
        public async Task<OperationResult<NewPostInternetDocumentDto>> CreateInternetDocument(int orderid, string SenderWarehouseIndex,string senderRef,
            string PayerType, string PaymentMethod, DateTimeOffset DateOfSend, float weight, string serviceType,string SeatsAmount,
            string description, float costOfEstimate,float costOfGood, CancellationToken cancellationToken)
        {
            var validationParam = CheckParameters(orderid, SenderWarehouseIndex,senderRef,PayerType,PaymentMethod,DateOfSend,weight,serviceType,
                SeatsAmount,description,costOfEstimate,costOfGood);
            if (!validationParam.Succeeded) { return new OperationResult<NewPostInternetDocumentDto>(false, validationParam.Message!); }

            var order = await orderService.GetOrderByIdAsync(orderid, cancellationToken);//take order
            if (order == null)  { return new OperationResult<NewPostInternetDocumentDto>(false, "Order does not exist"); }

            var customerInfo = order.Customer;//take customer info
            if (customerInfo == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Customer information is missing."); }

            var address = (await newPostCityesService.GetWarehousesAync(
                    CityName: customerInfo.City,
                    WarehouseId: null,
                    FindByString: customerInfo.DeliveryAddress,
                    CityRef: null,
                    Page: "1",
                    Limit: "50",
                    Language: null,
                    TypeOfWarehouseRef: null,
                    cancellationToken: cancellationToken)).FirstOrDefault();// take adress of wharehouse 
            if (address == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Warehouses serching exception"); }

            var counterAgentResult = await newPostCounerAgentService.CreateCounterpartyAsync(customerInfo, cancellationToken);//create counter agent
            if ((!counterAgentResult.Succeeded) || counterAgentResult.Payload == null || !counterAgentResult.Payload.Any(c => c != null))
            {
                var errorMessage = string.Join(Environment.NewLine, counterAgentResult.Errors);
                return new OperationResult<NewPostInternetDocumentDto>(false, $"Error creating counterparty: {errorMessage} or counterparty creation succeeded, but no data was returned.");
            }

            var counterAgent = counterAgentResult.Payload.FirstOrDefault(c => c.FirstName == customerInfo.FirstName && c.LastName == customerInfo.LastName);
            if (counterAgent == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Not true data about counterAgen"); }//take counter agent

            var counterAgentAdress = (await newPostCounerAgentService.GetCounterpartyAdressAsync(counterAgent.Ref, cancellationToken)).FirstOrDefault();
            if (counterAgentAdress == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Adress serching exception"); }//take CA adress

            var street = ExtractStreetName(address.ShortAddress);//give name of street from adress wharehouse 
            var build = ExtractBuildNumber(address.ShortAddress);//give nubmer of build
            var streetForUpdate = (await newPostCityesService.GetStreetsAync(address.CityRef, street, "1", "50", cancellationToken)).FirstOrDefault();
            if (streetForUpdate == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Street update exception"); }//update adress CA

            var updetedCounterAgent = await newPostCityesService.UpadateCounterPartyAdressAsync(counterAgent.Ref, counterAgentAdress.Ref,
                streetForUpdate.Ref, build, cancellationToken);//update CA
            if (!updetedCounterAgent.Succeeded) { return new OperationResult<NewPostInternetDocumentDto>(false, "Counteragent update exception");}

            var recipient = await newPostCounerAgentService.GetCounterpartyAsync(customerInfo, cancellationToken);
            if (recipient == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Recipient searching exception"); }

            var contactPersonOfReciooient = (await newPostCounerAgentService.GetContactPersonsAsync(recipient.Ref, "1", cancellationToken))
                .Where(c=>c.FirstName==recipient.FirstName && c.LastName==recipient.LastName).FirstOrDefault();
            var recipientAdress =(await newPostCounerAgentService.GetCounterpartyAdressAsync(recipient.Ref, cancellationToken)).FirstOrDefault();

            var sender = (await newPostCounerAgentService.GetSendersListAsync("1", cancellationToken)).Where(c=>c.Ref==senderRef).FirstOrDefault();
            if (sender == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Sender searching exception"); }

            var contactPersonofSender = (await newPostCounerAgentService.GetContactPersonsAsync(senderRef, "1", cancellationToken)).FirstOrDefault();//get contact person of sender
            var senderAdress = (await newPostCounerAgentService.GetCounterpartyAdressAsync(sender.Ref, cancellationToken)).FirstOrDefault();

            var requestBody = new
            {
                apiKey = _apiKey,
                modelName = "InternetDocument",
                calledMethod = "save",
                methodProperties = new
                {
                    SenderWarehouseIndex = SenderWarehouseIndex,
                    RecipientWarehouseIndex = address.WarehouseIndex,
                    PayerType = PayerType,
                    PaymentMethod = PaymentMethod,
                    DateTime = DateOfSend.ToString("dd.MM.yyyy"),
                    CargoType = "Parcel",
                    Weight = weight.ToString(),
                    ServiceType = serviceType,
                    SeatsAmount = SeatsAmount,
                    Description = description,
                    Cost = costOfEstimate.ToString(),
                    CitySender =sender.City,
                    Sender = sender.Ref,
                    SenderAddress = senderAdress!.Ref,
                    ContactSender = contactPersonofSender!.Ref,
                    SendersPhone = contactPersonofSender.Phones,
                    CityRecipient =address.Ref,
                    Recipient = recipient.Ref,
                    RecipientAddress = address.Ref,
                    ContactRecipient = contactPersonOfReciooient!.Ref,
                    RecipientsPhone= contactPersonOfReciooient.Phones,
                    BackwardDeliveryData = new[]
                    {
                        new
                        {
                            PayerType = "Recipient", 
                            CargoType = "Money",
                            RedeliveryString = costOfGood.ToString(),
                        }
                    }
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

                var apiResponse = JsonSerializer.Deserialize<NewPostResponseData<NewPostInternetDocumentDto>>(jsonResponse, _jsonSerializerOptions);
                if(apiResponse?.Data == null)
                {
                    _logger.LogError("Failed to retrieve data.");
                    return new OperationResult<NewPostInternetDocumentDto>(false, "Failed to retrieve data.");
                }
                var internetDocument = apiResponse.Data.FirstOrDefault();
                if (internetDocument == null){ return new OperationResult<NewPostInternetDocumentDto>(false, "Internet document not created.");}

                var newDocument = new NewPostInternetDocument
                {
                    Ref = apiResponse.Data.First().Ref,
                    CostOnSite = apiResponse.Data.First().CostOnSite,
                    EstimatedDeliveryDate = apiResponse.Data.First().EstimatedDeliveryDate,
                    IntDocNumber = apiResponse.Data.First().IntDocNumber,
                    TypeDocument = apiResponse.Data.First().TypeDocument,
                    OrderId = order.Id,
                };
                _dbContext.NewPostInternetDocuments.Add(newDocument);
                await _dbContext.SaveChangesAsync(cancellationToken);

                var resultDto = newDocument.ToNewPostInternetDocumentDto();
                return new OperationResult<NewPostInternetDocumentDto>(true, resultDto);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception occurred while creating Internet document.");
                return new OperationResult<NewPostInternetDocumentDto>(false, "Exception occurred while creating Internet document.");
            }
        }
        public async Task<OperationResult> DeleteInternetDocument(string internetDocumentRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(internetDocumentRef))
            {
                _logger.LogWarning("CounterPartyRef is null or empty.");
                return new OperationResult(false, "The internet document ref is empty");
            }
            var requestBody = new
            {
                apiKey = _apiKey,
                modelName = "InternetDocumentGeneral",
                calledMethod = "delete",
                methodProperties = new
                {
                    DocumentRefs = internetDocumentRef 
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
                var document = await _dbContext.NewPostInternetDocuments.FirstOrDefaultAsync(d => d.Ref == internetDocumentRef, cancellationToken);
                if (document != null)
                {
                    _dbContext.NewPostInternetDocuments.Remove(document);
                    await _dbContext.SaveChangesAsync(cancellationToken);
                    _logger.LogInformation("Internet Document with ref: {Ref} has been successfully deleted from the database.", internetDocumentRef);
                }
                else
                {
                    _logger.LogWarning("Internet Document with ref: {Ref} was not found in the database.", internetDocumentRef);
                    return new OperationResult(false, "Internet Document with ref: {Ref} was not found in the database.");
                }

                if (jsonResponse.Contains("\"success\":true"))
                {
                    return new OperationResult(true, "Internet document was deleted from both API and database.");
                }
                else
                {
                    _logger.LogError("Failed to delete internet document from Nova Poshta API.");
                    return new OperationResult(true, "Internet document was deleted from the database, but failed to delete from the API.");
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while deleting the internet document.");
                return new OperationResult(false, $"An error occurred: {ex.Message}");
            }
        }
        public async Task<IEnumerable<NewPostInternetDocumentDto>> GetAllInternetDocumentsAsync(CancellationToken cancellationToken)
        {
            var documents = await _dbContext.NewPostInternetDocuments.ToListAsync(cancellationToken);
            return documents.Select(d => d.ToNewPostInternetDocumentDto());
        }
        public async Task<NewPostInternetDocumentDto?> GetInternetDocumentByRefAsync(string documentRef, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(documentRef))
            {
                _logger.LogWarning("Document ref is null or empty.");
                return null;
            }
            var document = await _dbContext.NewPostInternetDocuments
                .FirstOrDefaultAsync(d => d.Ref == documentRef, cancellationToken);

            if (document == null)
            {
                _logger.LogWarning("Internet document with ref {Ref} not found.", documentRef);
                return null;
            }
            return document.ToNewPostInternetDocumentDto();
        }    
    }
}

