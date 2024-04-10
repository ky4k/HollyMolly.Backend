using HM.BLL.Models;
using HM.DAL.Entities;

namespace HM.BLL.Extensions;

public static class MappingExtensions
{
    public static UserDto ToUserDto(this User user)
    {
        return new UserDto()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            DeliveryAddress = user.DeliveryAddress,
        };
    }
    public static UserDto ToUserDto(this User user, IEnumerable<string> roles)
    {
        return new UserDto()
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            DateOfBirth = user.DateOfBirth,
            PhoneNumber = user.PhoneNumber,
            City = user.City,
            DeliveryAddress = user.DeliveryAddress,
            Roles = roles
        };
    }

    public static ProductDto ToProductDto(this Product product)
    {
        return new ProductDto()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Rating = product.Rating,
            Category = product.Category,
            StockQuantity = product.StockQuantity,
            Images = product.Images,
            Feedbacks = product.Feedbacks
        };
    }

    public static OrderDto ToOrderDto(this Order order)
    {
        var orderRecordsDto = new List<OrderRecordDto>();
        foreach(var orderRecord in order.OrderRecords)
        {
            orderRecordsDto.Add(orderRecord.ToOrderRecordDto());
        }
        return new OrderDto()
        {
            Id = order.Id,
            Customer = order.Customer.ToCustomerDto(),
            OrderDate = order.OrderDate,
            Status = order.Status,
            OrderRecords = orderRecordsDto
        };

    }

    public static CustomerDto ToCustomerDto(this CustomerInfo customer)
    {
        return new CustomerDto()
        {
            //Id = customer.Id,
            //OrderId = customer.OrderId,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PhoneNumber = customer.PhoneNumber,
            City = customer.City,
            DeliveryAddress = customer.DeliveryAddress,
        };
    }

    public static CustomerInfo ToCustomerInfo(this CustomerDto customer)
    {
        return new CustomerInfo()
        {
            //Id = customer.Id,
            //OrderId = customer.OrderId,
            Email = customer.Email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PhoneNumber = customer.PhoneNumber,
            City = customer.City,
            DeliveryAddress = customer.DeliveryAddress,
        };
    }

    public static OrderRecordDto ToOrderRecordDto(this OrderRecord orderRecord)
    {
        return new OrderRecordDto()
        {
            ProductId = orderRecord.ProductId,
            ProductName = orderRecord.ProductName,
            Quantity = orderRecord.Quantity,
            Price = orderRecord.Price
        };
    }
}
