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
        var images = new List<ProductImageDto>();
        foreach (var image in product.Images)
        {
            images.Add(image.ToProductImageDto());
        }
        return new ProductDto()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Price = product.Price,
            Rating = product.Rating,
            Category = product.Category.Name,
            StockQuantity = product.StockQuantity,
            Images = images,
            Feedbacks = product.Feedbacks
        };
    }

    private static ProductImageDto ToProductImageDto(this ProductImage productImage)
    {
        return new ProductImageDto()
        {
            Id = productImage.Id,
            Link = productImage.Link
        };
    }

    public static OrderDto ToOrderDto(this Order order)
    {
        var orderRecordsDto = new List<OrderRecordDto>();
        foreach (var orderRecord in order.OrderRecords)
        {
            orderRecordsDto.Add(orderRecord.ToOrderRecordDto());
        }
        return new OrderDto()
        {
            Id = order.Id,
            Customer = order.Customer.ToCustomerDto(),
            OrderDate = order.OrderDate,
            Status = order.Status,
            Notes = order.Notes,
            OrderRecords = orderRecordsDto
        };

    }

    public static CustomerDto ToCustomerDto(this CustomerInfo customer)
    {
        return new CustomerDto()
        {
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

    public static CategoryGroupDto ToCategoryGroupDto(this CategoryGroup categoryGroup)
    {
        List<CategoryDto> categoriesDto = [];
        foreach (var category in categoryGroup.Categories)
        {
            categoriesDto.Add(category.ToCategoryDto());
        }
        var categoryGroupDto = new CategoryGroupDto()
        {
            Id = categoryGroup.Id,
            Name = categoryGroup.Name,
            Link = categoryGroup.ImageLink,
            Categories = categoriesDto
        };
        return categoryGroupDto;
    }

    public static CategoryDto ToCategoryDto(this Category category)
    {
        return new CategoryDto()
        {
            Id = category.Id,
            CategoryGroupId = category.CategoryGroupId,
            Name = category.Name,
            Link = category.ImageLink
        };
    }
}
