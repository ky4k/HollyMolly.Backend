using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Stripe.Checkout;

namespace HM.BLL.Services;

public class CheckoutService(
    HmDbContext context,
    SessionService sessionService,
    ILogger<CheckoutService> logger
    ) : ICheckoutService
{
    public async Task<OperationResult<string>> PayForOrderAsync(int orderId, string userId,
        string baseUrl, CancellationToken cancellationToken)
    {
        Order? order = await context.Orders
            .Include(o => o.OrderRecords)
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order == null)
        {
            return new OperationResult<string>(false, "No order with such an id.", null!);
        }
        if (order.UserId != userId)
        {
            return new OperationResult<string>(false, "You may pay only for your orders.", null!);
        }
        if (order.PaymentReceived)
        {
            return new OperationResult<string>(false, "The order has already been paid for.", null!);
        }
        decimal total = 0;
        List<SessionLineItemOptions> lineItemOptions = [];
        foreach (OrderRecord record in order.OrderRecords)
        {
            ProductInstance? productInstance = await context.Products
                .Include(p => p.ProductInstances)
                .SelectMany(p => p.ProductInstances)
                .FirstOrDefaultAsync(pi => pi.Id == record.ProductInstanceId, cancellationToken);
            List<string> images = [];
            foreach (ProductImage image in productInstance?.Images ?? [])
            {
                string link = Uri.EscapeDataString(image.Link);
                if (!string.IsNullOrEmpty(link))
                {
                    images.Add(link);
                }
            }
            if (record.Quantity == 0)
            {
                continue;
            }
            decimal pricePerUnit = record.Price - (record.Discount / record.Quantity);
            total += pricePerUnit * record.Quantity;

            var itemPriceDataOptions = new SessionLineItemOptions()
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(100 * pricePerUnit), // Convert to cents
                    Currency = "UAH",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = record.ProductName,
                    }
                },
                Quantity = record.Quantity
            };
            if (images.Count > 0)
            {
                itemPriceDataOptions.PriceData.ProductData.Images = images;
            }

            lineItemOptions.Add(itemPriceDataOptions);
        }
        if (total < 20)
        {
            return new OperationResult<string>(false, "Sorry, the payment system accepts payment " +
                $"starting on 20 hryvnias. Your total cost is {total} hryvnias.");
        }

        var options = new SessionCreateOptions()
        {
            SuccessUrl = $"{baseUrl}/api/Checkout/success?sessionId=" + "{CHECKOUT_SESSION_ID}",
            CancelUrl = $"{baseUrl}/api/Checkout/failed",
            PaymentMethodTypes = ["card"],
            Metadata = new Dictionary<string, string>() { { "orderId", $"{order.Id}" } },
            LineItems = lineItemOptions,
            Mode = "payment"
        };

        try
        {
            Session session = await sessionService.CreateAsync(options, cancellationToken: cancellationToken);
            return new OperationResult<string>(true, "", session.Url);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Cannot create Stripe session with options: {options}", options);
            return new OperationResult<string>(false, "Payment system was unable to process the payment.");
        }
    }

    public async Task<OperationResult> CheckoutSuccessAsync(string sessionId)
    {
        Session session = await sessionService.GetAsync(sessionId);
        if (session.PaymentStatus == "paid")
        {
            int orderId = int.Parse(session.Metadata["orderId"]);
            Order? order = await context.Orders
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return new OperationResult(false, "Payment was not processed correctly. "
                    + "Contact the support for the details.");
            }
            try
            {
                order.Status = "Payment Received";
                order.PaymentReceived = true;
                context.Orders.Update(order);
                await context.SaveChangesAsync();
                return new OperationResult(true);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Order was not updated after receiving payment: {order}", order);
                return new OperationResult(false, "Payment was not processed correctly. "
                    + "Contact the support for the details.");
            }
        }
        return new OperationResult(false, "The payment system has not confirmed the payment for the order.");
    }
}
