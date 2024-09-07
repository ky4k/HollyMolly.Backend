using DocumentFormat.OpenXml.Drawing.Charts;
using DocumentFormat.OpenXml.Presentation;
using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;
using HM.DAL.Data;
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

        public async Task<OperationResult<NewPostInternetDocumentDto>> CreateInternetDocument(int orderid, string typeOfSender,
            DateTimeOffset date, float weight, string serviceType, string description, float cost, CancellationToken cancellationToken)
        {
            var order = await orderService.GetOrderByIdAsync(orderid, cancellationToken);//take order
            if (order == null)
            {
                return new OperationResult<NewPostInternetDocumentDto>(false, "Order does not exist");
            }
            var customerInfo = order.Customer;//take customer info

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
            if (address==null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Warehouses serching exception"); }

            var street = ExtractStreetName(address.ShortAddress);//give name of street from adress wharehouse 
            var streetForUpdate = (await newPostCityesService.GetStreetsAync(address.CityRef, street, "1", "50", cancellationToken)).FirstOrDefault();
            if (streetForUpdate == null) { return new OperationResult<NewPostInternetDocumentDto>(false, "Street update exception"); }//update adress CA

            var updetedCounterAgent = newPostCityesService.UpadateCounterPartyAdress(counterAgent.Ref, counterAgentAdress.Ref, 
                streetForUpdate.Ref, null, null,null, cancellationToken);//update CA

            if (updetedCounterAgent.IsFaulted) { return new OperationResult<NewPostInternetDocumentDto>(false, "Counteragent update exception"); }

            var requestbody = new
            {
                apiKey = _apiKey,
                modelName = "InternetDocument",
                calledMethod = "save",
                methodProperties = new
                {
                    SenderWarehouseIndex = "",// не обовязково
                    RecipientWarehouseIndex = address.WarehouseIndex,
                    PayerType = typeOfSender,
                    PaymentMethod = "Cash",
                    DateTime = date.ToString(),
                    CargoType = order.OrderRecords.FirstOrDefault()?.ProductName ?? "Мода та краса",
                    Weight = weight,
                    ServiceType =  serviceType,
                    SeatsAmount = "1",
                    description = description,
                    cost = cost,
                    CitySender = 
                }
            };
        }
    }
}

