﻿using HM.BLL.Interfaces;
using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;
using HM.BLL.Services;
using HM.BLL.Services.NewPost;
using HM.DAL.Entities.NewPost;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class NewPostController(
    INewPostCityesService newPostService,
    INewPostCounerAgentService newPostCounterAgentService,
    INewPostInternetDocument newPostInternetDocument,
    IEmailService emailSevice,
    IOrderService orderService
    ) : ControllerBase
{
    /// <summary>
    /// Allows to get a list of cities from the New Post.
    /// </summary>
    /// <param name="findByString">Optional. Part of the city name to search for.</param>
    /// <param name="ref">Optional. Reference of the city.</param>
    /// <param name="page">Optional. Page number of cities. Defaults to 1.</param>
    /// <param name="limit">Optional. Number of items per page. Defaults to 100.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the page that contains a list of cities.</response>
    /// <response code="400">Indicates that cities cannot be obtained and returns the error message.</response>
    [Route("cities")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostCities>>> GetCities(
        string? findByString = null,
        string? @ref = null,
        int page = 1,
        int limit = 100,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await newPostService.GetCitiesAsync(
            FindByString: findByString,
            Ref: @ref,
            Page: page.ToString(),
            Limit: limit.ToString(),
            cancellationToken: cancellationToken);

            if (result == null || !result.Any())
            {
                return BadRequest("Cities cannot be obtained.");
            }

            return Ok(result);
        }
        catch (Exception ex) { return BadRequest($"Error: {ex.Message}"); }
    }

    /// <summary>
    /// Allows to get a list of warehouses from the New Post in the specified city.
    /// </summary>
    /// <param name="cityName">Optional. Name of the city to search within.</param>
    /// <param name="warehouseId">Optional. ID of the specific warehouse to search for.</param>
    /// <param name="findByString">Optional. Part of the warehouse name to search for.</param>
    /// <param name="cityRef">Optional. Reference of the city obtained from NewPost/cities endpoint.</param>
    /// <param name="page">Optional. Page number of warehouses. Defaults to 1.</param>
    /// <param name="limit">Optional. Number of items per page. Defaults to 100.</param>
    /// <param name="language">Optional. Language for the response.</param>
    /// <param name="typeOfWarehouseRef">Optional. Type of the warehouse reference.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the page that contains a list of warehouses in the city (up to 100).</response>
    /// <response code="400">Indicates that warehouses cannot be obtained and returns the error message.</response>
    [Route("warehouses")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostWarehouse>>> GetWarehouses(
        string? cityName = null,
        string? warehouseId = null,
        string? findByString = null,
        string? cityRef = null,
        int page = 1,
        int limit = 100,
        string? language = null,
        string? typeOfWarehouseRef = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await newPostService.GetWarehousesAync(
            CityName: cityName,
            WarehouseId: warehouseId,
            FindByString: findByString,
            CityRef: cityRef,
            Page: page.ToString(),
            Limit: limit.ToString(),
            Language: language,
            TypeOfWarehouseRef: typeOfWarehouseRef,
            cancellationToken: cancellationToken);

            if (result == null || !result.Any())
            {
                return BadRequest("Cities cannot be obtained.");
            }

            return Ok(result);
        }
        catch (Exception ex) { return BadRequest($"Error: {ex.Message}"); }
        
    }

    /// <summary>
    /// Allows to create a counterparty in the New Post service.
    /// </summary>
    /// <param name="customerDto">The customer data used to create the counterparty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the created counterparty.</response>
    /// <response code="400">Indicates that the counterparty could not be created and returns the error message.</response>
    [Route("counteragents")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<NewPostCounterAgentDto>> CreateCounterparty(
        [FromBody] CustomerDto customerDto,
        CancellationToken cancellationToken = default)
    {
        var result = await newPostCounterAgentService.CreateCounterpartyAsync(customerDto, cancellationToken);

        return result.Succeeded ? Ok(result.Payload) : BadRequest(result.Message);
    }
    /// <summary>
    /// Allows to retrieve a counterparty by customer data.
    /// </summary>
    /// <param name="customerDto">The customer data used to find the counterparty.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the counterparty.</response>
    /// <response code="404">Indicates that the counterparty was not found and returns the error message.</response>
    [Route("counterparties/get")]
    [HttpPost]
    public async Task<IActionResult> GetCounterpartyAsync([FromBody] CustomerDto customerDto, CancellationToken cancellationToken)
    {
        try
        {
            var counterpartyDto = await newPostCounterAgentService.GetCounterpartyAsync(customerDto, cancellationToken);
            return Ok(counterpartyDto);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred.", details = ex.Message });
        }
    }

    /// <summary>
    /// Allows to get a list of streets from the New Post based on the city reference.
    /// </summary>
    /// <param name="cityRef">Reference of the city obtained from NewPost/cities endpoint.</param>
    /// <param name="findByString">Optional. Part of the street name to search for.</param>
    /// <param name="page">Optional. Page number of streets. Defaults to 1.</param>
    /// <param name="limit">Optional. Number of items per page. Defaults to 50.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the page that contains a list of streets.</response>
    /// <response code="400">Indicates that streets cannot be obtained and returns the error message.</response>
    [Route("streets")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostStreets>>> GetStreets(
        string cityRef,
        string? findByString = null,
        int page = 1,
        int limit = 50,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await newPostService.GetStreetsAync(
            CityRef: cityRef,
            FindByString: findByString ?? string.Empty,
            Page: page.ToString(),
            Limit: limit.ToString(),
            cancellationToken: cancellationToken);

            if (result == null || !result.Any())
            {
                return BadRequest("Streets cannot be obtained.");
            }

            return Ok(result);
        }
        catch (Exception ex) { return BadRequest($"Error: {ex.Message}"); }
    }

    /// <summary>
    /// Allows to get a list of counterparty addresses from the New Post based on the counterparty reference and property.
    /// </summary>
    /// <param name="counterPartyRef">Reference of the counterparty obtained from Nova Poshta API.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the page that contains a list of counterparty addresses.</response>
    /// <response code="400">Indicates that counterparty addresses cannot be obtained and returns the error message.</response>
    [Route("counterpartyaddresses")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostCounterAgentAdress>>> GetCounterpartyAddresses(
        string counterPartyRef, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(counterPartyRef))
        {
            return BadRequest("CounterPartyRef is required.");
        }

        try
        {
            var addresses = await newPostCounterAgentService.GetCounterpartyAdressAsync(counterPartyRef, cancellationToken);

            if (addresses == null || !addresses.Any())
            {
                // Якщо результат порожній, повертаємо NotFound
                return NotFound("No addresses found for the provided counterPartyRef.");
            }

            return Ok(addresses);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
        }
    }
    /// <summary>
    /// Allows to create a list of counterparties from the New Post service based on the specified counterparty property.
    /// </summary>
    /// <param name="counterpartyProperty">The counterparty property to filter by (e.g., "Sender", "Recipient").</param>
    /// <param name="page">Optional. The page number of counterparties to retrieve. Defaults to 1.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns a list of counterparties based on the specified criteria.</response>
    /// <response code="400">Indicates that counterparties cannot be obtained and returns the error message.</response>
    [Route("counterparties/list")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostCounterAgents>>> GetSendersList(
        int page = 1,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await newPostCounterAgentService.GetSendersListAsync(
                Page: page.ToString(),
                cancellationToken: cancellationToken);

            if (result == null || !result.Any())
            {
                return BadRequest("Counterparties cannot be obtained.");
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred.", details = ex.Message });
        }
    }
    /// <summary>
    /// Retrieves a list of contact persons associated with a specific counterparty.
    /// </summary>
    /// <param name="counterPartyRef">Reference of the counterparty obtained from Nova Poshta API.</param>
    /// <param name="page">Optional. Page number of contact persons. Defaults to 1.</param>
    /// <param name="cancellationToken">A cancellation token that can be used to cancel the operation.</param>
    /// <response code="200">Returns the list of contact persons associated with the counterparty.</response>
    /// <response code="400">Indicates that contact persons cannot be obtained and returns the error message.</response>
    [Route("contactpersons")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IEnumerable<NewPostContactPersonDto>>> GetContactPersons(
        string counterPartyRef,
        string page = "1",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(counterPartyRef))
        {
            return BadRequest("CounterPartyRef is required.");
        }

        try
        {
            var contactPersons = await newPostCounterAgentService.GetContactPersonsAsync(counterPartyRef, page, cancellationToken);

            if (contactPersons == null || !contactPersons.Any())
            {
                return NotFound("No contact persons found for the provided counterPartyRef.");
            }

            return Ok(contactPersons);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred.", details = ex.Message });
        }
    }
    /// <summary>
    /// Creates an internet document in New Post.
    /// </summary>
    /// <param name="orderid">Order ID.</param>
    /// <param name="SenderWarehouseIndex">Sender warehouse index.</param>
    /// <param name="senderRef">Sender reference.</param>
    /// <param name="PayerType">Payer type.</param>
    /// <param name="PaymentMethod">Payment method.</param>
    /// <param name="DateOfSend">Date of sending.</param>
    /// <param name="CargoType">Cargo type.</param>
    /// <param name="weight">Weight of the cargo.</param>
    /// <param name="serviceType">Service type.</param>
    /// <param name="SeatsAmount">Number of seats.</param>
    /// <param name="description">Description of the cargo.</param>
    /// <param name="cost">Cost of the cargo.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>Returns the created internet document or an error message.</returns>
    [Route("internet-document")]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInternetDocument(
    int orderId,
    string? senderWarehouseIndex,
    string senderRef,
    string payerType,
    string paymentMethod,
    DateTimeOffset dateOfSend,
    float weight,
    string serviceType,
    string seatsAmount,
    string description,
    float cost,
    float costOfGood,
    CancellationToken cancellationToken)
    {
        var result = await newPostInternetDocument.CreateInternetDocument(
            orderId,
            senderWarehouseIndex,
            senderRef,
            payerType,
            paymentMethod,
            dateOfSend,
            weight,
            serviceType,
            seatsAmount,
            description,
            cost,
            costOfGood,
            cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result.Message);
        }
        else
        {
            var order = await orderService.GetOrderByIdAsync(orderId, cancellationToken);
            var documentNumber = result.Payload?.IntDocNumber;
            if (!string.IsNullOrEmpty(documentNumber))
            {
                await emailSevice.SendInternetDocumentCreatedEmailAsync(order, documentNumber, cancellationToken);
            }
        }

        return Ok(result.Payload);
    }
    [Route("internet-document/delete")]
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteInternetDocument(
        [FromQuery] string internetDocumentRef,
        CancellationToken cancellationToken = default)
    {
        var result = await newPostInternetDocument.DeleteInternetDocument(internetDocumentRef, cancellationToken);

        if (result.Succeeded)
            return Ok(result.Message);

        return BadRequest(result.Message);
    }

    [Route("internet-documents")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetAllInternetDocuments(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await newPostInternetDocument.GetAllInternetDocumentsAsync(cancellationToken);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest($"Error: {ex.Message}");
        }
    }

    [Route("internet-document/{documentRef}")]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInternetDocumentByRef(
        [FromRoute] string documentRef,
        CancellationToken cancellationToken = default)
    {
        var result = await newPostInternetDocument.GetInternetDocumentByRefAsync(documentRef, cancellationToken);

        if (result == null)
            return NotFound($"Internet document with ref {documentRef} not found.");

        return Ok(result);
    }
}
