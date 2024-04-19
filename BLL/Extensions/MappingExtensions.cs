using HM.BLL.Models;
using HM.DAL.Entities;
using HM.DAL.Migrations;

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
        List<ProductInstanceDto> productInstancesDto = [];
        foreach (HM.DAL.Entities.ProductInstance productInstance in product.ProductInstances)
        {
            productInstancesDto.Add(productInstance.ToProductInstanceDto());
        }
        return new ProductDto()
        {
            Id = product.Id,
            Name = product.Name,
            Description = product.Description,
            Rating = product.Rating,
            TimesRated = product.TimesRated,
            CategoryId = product.Category.Id,
            ProductsInstances = productInstancesDto,
            Feedbacks = product.Feedbacks
        };
    }

    public static ProductInstanceDto ToProductInstanceDto(this HM.DAL.Entities.ProductInstance productInstance)
    {
        var images = new List<ProductImageDto>();
        foreach (var image in productInstance.Images)
        {
            images.Add(image.ToProductImageDto());
        }
        return new ProductInstanceDto()
        {
            Id = productInstance.Id,
            StockQuantity = productInstance.StockQuantity,
            Price = productInstance.Price,
            SKU = productInstance.SKU,
            Color = productInstance.Color,
            Size = productInstance.Size,
            Discount = productInstance.Discount?.ToDiscountDto(),
            Images = images
        };
    }

    public static DiscountDto ToDiscountDto(this Discount discount)
    {
        return new DiscountDto()
        {
            Id = discount.Id,
            AbsoluteDiscount = discount.AbsoluteDiscount,
            PercentageDiscount = discount.PercentageDiscount
        };
    }

    private static ProductImageDto ToProductImageDto(this HM.DAL.Entities.ProductImage productImage)
    {
        return new ProductImageDto()
        {
            Id = productImage.Id,
            Position = productImage.Position,
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
            ProductInstanceId = orderRecord.ProductInstanceId,
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
    public static WishListDto ToWishListDto(this HM.DAL.Entities.WishList wishList)
    {
        return new WishListDto()
        {
            Id = wishList.Id,
            UserId = wishList.UserId,
            User = wishList.User.ToUserDto(),
            Products = wishList.Products.Select(p => p.ToProductDto()).ToList()
        };
    }
}
