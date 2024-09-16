﻿using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Presentation;
using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;
using HM.DAL.Data;
using HM.DAL.Entities.NewPost;
using Microsoft.Extensions.Logging;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HM.BLL.Services.NewPost
{
    public class NewPostInternetDocumentSertvice(
    HmDbContext _dbContext,
    IConfigurationHelper configurationHelper,
    IHttpClientFactory httpClientFactory,
    ILogger<NewPostCounterAgentService> logger,
    IOrderService orderService,
    INewPostCounerAgentService newPostCounerAgentService,
    INewPostCityesService newPostCityesService) : INewPostInternetDocument
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

        public async Task<OperationResult<NewPostInternetDocumentDto>> CreateInternetDocument(int orderid, string? SenderWarehouseIndex,string senderRef,
            string PayerType, string PaymentMethod, DateTimeOffset DateOfSend, float weight, string serviceType,string SeatsAmount,
            string description, float cost, CancellationToken cancellationToken)
        {
            var order = await orderService.GetOrderByIdAsync(orderid, cancellationToken);//take order
            if (order == null) 
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Order does not exist");
            }
            var customerInfo = order.Customer;//take customer info

            var address = (await newPostCityesService.GetWarehousesAync(
                    CityName: customerInfo.City,
                    WarehouseId: null,
                    FindByString: customerInfo.DeliveryAddress,
                    CityRef: null,
                    Page: "1",
                    Limit: "50",
                    Language: null,
                    TypeOfWarehouseRef: null,
                    cancellationToken: cancellationToken))
                    .FirstOrDefault();// take adress of wharehouse 
            if (address == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Warehouses serching exception"); }

            if (customerInfo != null)
            {
                var counterAgentResult = await newPostCounerAgentService.CreateCounterpartyAsync(customerInfo, cancellationToken);//create counter agent
                if (!counterAgentResult.Succeeded)
                {
                    var errorMessage = string.Join(Environment.NewLine, counterAgentResult.Errors);
                    return new OperationResult<NewPostInternetDocumentDto>(false, errorMessage);
                } 

                var counterAgent = counterAgentResult.Payload.FirstOrDefault(c => c.FirstName == customerInfo.FirstName && c.LastName == customerInfo.LastName);
                if (counterAgent == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Not true data about con"); }//take counter agent

                var counterAgentAdress = (await newPostCounerAgentService.GetCounterpartyAdressAsync(counterAgent.Ref, cancellationToken)).FirstOrDefault();
                if (counterAgentAdress == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Adress serching exception"); }//take CA adress

                var street = ExtractStreetName(address.ShortAddress);//give name of street from adress wharehouse 
                var build = ExtractBuildNumber(address.ShortAddress);//give nubmer of build
                var streetForUpdate = (await newPostCityesService.GetStreetsAync(address.CityRef, street, "1", "50", cancellationToken)).FirstOrDefault();
                if (streetForUpdate == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Street update exception"); }//update adress CA

                var updetedCounterAgent = await newPostCityesService.UpadateCounterPartyAdressAsync(counterAgent.Ref, counterAgentAdress.Ref,
                    streetForUpdate.Ref, build, cancellationToken);//update CA

                if (!updetedCounterAgent.Succeeded)
                {
                    return new OperationResult<NewPostInternetDocumentDto>(false, "Counteragent update exception");
                }
            }
            else { return new OperationResult<NewPostInternetDocumentDto>(false, "Customer info dosen't serched"); }

            var recipient = await newPostCounerAgentService.GetCounterpartyAsync(customerInfo, cancellationToken);
            var contactPersonOfReciooient = (await newPostCounerAgentService.GetContactPersonsAsync(recipient.Ref, "1", cancellationToken))
                .Where(c=>c.FirstName==recipient.FirstName && c.LastName==recipient.LastName).FirstOrDefault();
            var recipientAdress =(await newPostCounerAgentService.GetCounterpartyAdressAsync(recipient.Ref, cancellationToken)).FirstOrDefault();

            var sender = (await newPostCounerAgentService.GetSendersListAsync("1", cancellationToken)).Where(c=>c.Ref==senderRef).FirstOrDefault();
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
                    Cost = cost.ToString(),
                    CitySender =sender.City,
                    Sender = sender.Ref,
                    SenderAddress = senderAdress.Ref,
                    ContactSender = contactPersonofSender.Ref,
                    SendersPhone = contactPersonofSender.Phones,
                    CityRecipient =address.Ref,
                    Recipient = recipient.Ref,
                    RecipientAddress = address.Ref,
                    ContactRecipient = contactPersonOfReciooient.Ref,
                    RecipientsPhone= contactPersonOfReciooient.Phones
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

                if(apiResponse.Data == null)
                {
                    _logger.LogError("Failed to retrieve data.");
                    return new OperationResult<NewPostInternetDocumentDto>(false, "Failed to retrieve data.");
                }
                var internetDocument = apiResponse.Data.FirstOrDefault();
                if (internetDocument == null)
                {
                    return new OperationResult<NewPostInternetDocumentDto>(false, "Internet document not created.");
                }

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
    }
}

