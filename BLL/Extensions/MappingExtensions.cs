using HM.BLL.Models.Categories;
using HM.BLL.Models.Orders;
using HM.BLL.Models.Products;
using HM.BLL.Models.Supports;
using HM.BLL.Models.Users;
using HM.BLL.Models.WishLists;
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
            Rating = product.Rating,
            TimesRated = product.TimesRated,
            CategoryId = product.CategoryId,
            ProductsInstances = product.ProductInstances.Select(pi => pi.ToProductInstanceDto()).ToList(),
            Feedbacks = product.Feedbacks.Select(pf => pf.ToProductFeedbackDto()).ToList()
        };
    }

    public static ProductInstanceDto ToProductInstanceDto(this ProductInstance productInstance)
    {
        var images = new List<ProductImageDto>();
        foreach (ProductImage image in productInstance.Images)
        {
            images.Add(image.ToProductImageDto());
        }
        return new ProductInstanceDto()
        {
            Id = productInstance.Id,
            StockQuantity = productInstance.StockQuantity,
            Price = productInstance.Price,
            Status = productInstance.Status,
            IsNewCollection = productInstance.IsNewCollection,
            SKU = productInstance.SKU,
            Color = productInstance.Color,
            Size = productInstance.Size,
            Material = productInstance.Material,
            AbsoluteDiscount = productInstance.AbsoluteDiscount,
            PercentageDiscount = productInstance.PercentageDiscount,
            Images = images
        };
    }

    public static ProductInstance ToProductInstance(this ProductInstanceCreateDto productInstanceDto)
    {
        return new ProductInstance()
        {
            StockQuantity = productInstanceDto.StockQuantity,
            Price = productInstanceDto.Price,
            Status = productInstanceDto.Status,
            IsNewCollection = productInstanceDto.IsNewCollection,
            SKU = productInstanceDto.SKU,
            Color = productInstanceDto.Color,
            Size = productInstanceDto.Size,
            Material = productInstanceDto.Material,
            AbsoluteDiscount = productInstanceDto.AbsoluteDiscount,
            PercentageDiscount = productInstanceDto.PercentageDiscount
        };
    }

    public static ProductFeedbackDto ToProductFeedbackDto(this ProductFeedback productFeedback)
    {
        return new ProductFeedbackDto()
        {
            Id = productFeedback.Id,
            ProductId = productFeedback.ProductId,
            AuthorName = productFeedback.AuthorName,
            Created = productFeedback.Created,
            Rating = productFeedback.Rating,
            Review = productFeedback.Review
        };
    }

    private static ProductImageDto ToProductImageDto(this ProductImage productImage)
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
        return new OrderDto()
        {
            Id = order.Id,
            Customer = order.Customer.ToCustomerDto(),
            OrderDate = order.OrderDate,
            Status = order.Status,
            PaymentReceived = order.PaymentReceived,
            Notes = order.Notes,
            OrderRecords = order.OrderRecords.Select(or => or.ToOrderRecordDto()).ToList()
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
            Price = orderRecord.Price,
            Discount = orderRecord.Discount
        };
    }

    public static CategoryGroupDto ToCategoryGroupDto(this CategoryGroup categoryGroup)
    {
        return new CategoryGroupDto()
        {
            Id = categoryGroup.Id,
            Name = categoryGroup.Name,
            Position = categoryGroup.Position,
            Link = categoryGroup.ImageLink,
            Categories = categoryGroup.Categories.Select(c => c.ToCategoryDto()).ToList()
        };
    }

    public static CategoryDto ToCategoryDto(this Category category)
    {
        return new CategoryDto()
        {
            Id = category.Id,
            CategoryGroupId = category.CategoryGroupId,
            Name = category.Name,
            Position = category.Position,
            Link = category.ImageLink
        };
    }
    public static WishListDto ToWishListDto(this WishList wishList)
    {
        return new WishListDto()
        {
            Id = wishList.Id,
            UserId = wishList.UserId,
            Products = wishList.Products.Select(p => p.ToProductDto()).ToList()
        };
    }
    public static Support ToSupport(this SupportCreateDto supportDto)
    {
        return new Support
        {
            Name = supportDto.Name,
            Email = supportDto.Email,
            Topic = supportDto.Topic,
            Description = supportDto.Description,
            OrderId = supportDto.OrderId,
            ReceivedAt = DateTimeOffset.UtcNow
        };
    }
}
