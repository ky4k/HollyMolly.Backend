using HM.BLL.Models.Categories;
using HM.BLL.Models.NewPost;
using HM.BLL.Models.Orders;
using HM.BLL.Models.Products;
using HM.BLL.Models.Supports;
using HM.BLL.Models.Users;
using HM.BLL.Models.WishLists;
using HM.DAL.Entities;
using HM.DAL.Entities.NewPost;

namespace HM.BLL.Extensions;

public static class MappingExtensions
{
    public static UserDto ToUserDto(this User user)
    {
        return new UserDto()
        {
            Id = user.Id,
            Email = user.Email,
            Profiles = user.Profiles.Select(p => p.ToProfileDto()).ToList()
        };
    }
    public static UserDto ToUserDto(this User user, IEnumerable<string> roles)
    {
        UserDto userDto = user.ToUserDto();
        userDto.Roles = roles.ToList();
        return userDto;
    }
    public static ProfileDto ToProfileDto(this Profile profile)
    {
        return new ProfileDto()
        {
            Id = profile.Id,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            DateOfBirth = profile.DateOfBirth,
            PhoneNumber = profile.PhoneNumber,
            City = profile.City,
            DeliveryAddress = profile.DeliveryAddress
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
        return new ProductInstanceDto()
        {
            Id = productInstance.Id,
            StockQuantity = productInstance.StockQuantity,
            Price = productInstance.Price,
            Status = productInstance.Status ?? GetStatus(productInstance),
            IsNewCollection = productInstance.IsNewCollection,
            SKU = productInstance.SKU,
            Color = productInstance.Color,
            Size = productInstance.Size,
            Material = productInstance.Material,
            AbsoluteDiscount = productInstance.AbsoluteDiscount,
            PercentageDiscount = productInstance.PercentageDiscount,
            Images = productInstance.Images.Select(im => im.ToProductImageDto()).ToList()
        };
    }
    private static string GetStatus(ProductInstance productInstance)
    {
        string? status = null;
        switch (productInstance.StockQuantity)
        {
            case <= 0:
                status = "Немає в наявності";
                break;
            case <= 10:
                status = "Закінчується";
                break;
            case > 10:
                status = "В наявності";
                break;
        }
        return status;
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
            StatusHistory = order.StatusHistory.Select(s => s.ToOrderStatusHistoryDto()).ToList(),
            PaymentReceived = order.PaymentReceived,
            Notes = order.Notes,
            OrderRecords = order.OrderRecords.Select(or => or.ToOrderRecordDto()).ToList(),
            NewPostDocument = order.NewPostInternetDocument?.IntDocNumber
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

    public static CustomerInfo ToCustomerInfo(this CustomerCreateDto customer, string email)
    {
        return new CustomerInfo()
        {
            Email = email,
            FirstName = customer.FirstName,
            LastName = customer.LastName,
            PhoneNumber = customer.PhoneNumber,
            City = customer.City,
            DeliveryAddress = customer.DeliveryAddress,
        };
    }
    public static OrderStatusHistoryDto ToOrderStatusHistoryDto(this OrderStatusHistory orderStatus)
    {
        return new OrderStatusHistoryDto()
        {
            Status = orderStatus.Status,
            Date = orderStatus.Date,
            Notes = orderStatus.Notes
        };
    }
    public static OrderRecordDto ToOrderRecordDto(this OrderRecord orderRecord)
    {
        return new OrderRecordDto()
        {
            ProductInstanceId = orderRecord.ProductInstanceId,
            ProductSKU = orderRecord.ProductInstance.SKU,
            ProductName = orderRecord.ProductName,
            Size = orderRecord.ProductInstance?.Size,
            Color = orderRecord.ProductInstance?.Color,
            Image = orderRecord.ProductInstance?.Images
                .DefaultIfEmpty().MinBy(i => i?.Position)?.Link,
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
    public static NewPostCounterAgentDto ToNewPostCounterAgentDto(this NewPostCounterAgent counterAgent)
    {
        return new NewPostCounterAgentDto
        {
            Ref = counterAgent.Ref,
            Description = counterAgent.Description,
            FirstName = counterAgent.FirstName,
            MiddleName = counterAgent.MiddleName,
            LastName = counterAgent.LastName,
            Counterparty = counterAgent.Counterparty,
            OwnershipForm = counterAgent.OwnershipForm,
            OwnershipFormDescription = counterAgent.OwnershipFormDescription,
            EDRPOU = counterAgent.EDRPOU,
            CounterpartyType = counterAgent.CounterpartyType,
            ContactPerson = new NewPostResponseData<NewPostContactPersonDto>
            {
                Data = counterAgent.ContactPersons.Select(cp => cp.ToNewPostContactPersonDto()).ToList()
            }
        };
    }
    public static NewPostCounterAgent ToNewPostCounterAgent(this NewPostCounterAgentDto counterAgentDto)
    {
        return new NewPostCounterAgent
        {
            Ref = counterAgentDto.Ref,
            Description = counterAgentDto.Description,
            FirstName = counterAgentDto.FirstName,
            MiddleName = counterAgentDto.MiddleName,
            LastName = counterAgentDto.LastName,
            Counterparty = counterAgentDto.Counterparty,
            OwnershipForm = counterAgentDto.OwnershipForm,
            OwnershipFormDescription = counterAgentDto.OwnershipFormDescription,
            EDRPOU = counterAgentDto.EDRPOU,
            CounterpartyType = counterAgentDto.CounterpartyType,
            ContactPersons = counterAgentDto.ContactPerson.Data
                .Select(cp => cp.ToNewPostContactPerson())
                .ToList()
        };
    }
    public static NewPostContactPersonDto ToNewPostContactPersonDto(this NewPostContactPerson contactPerson)
    {
        return new NewPostContactPersonDto
        {
            Ref = contactPerson.Ref,
            Description = contactPerson.Description,
            LastName = contactPerson.LastName,
            FirstName = contactPerson.FirstName,
            MiddleName = contactPerson.MiddleName,
            Email = contactPerson.Email,
            Phones = contactPerson.Phones
        };
    }
    public static NewPostContactPerson ToNewPostContactPerson(this NewPostContactPersonDto contactPersonDto)
    {
        return new NewPostContactPerson
        {
            Ref = contactPersonDto.Ref,
            Description = contactPersonDto.Description,
            LastName = contactPersonDto.LastName,
            FirstName = contactPersonDto.FirstName,
            MiddleName = contactPersonDto.MiddleName,
            Email = contactPersonDto.Email,
            Phones = contactPersonDto.Phones
        };
    }
    public static NewPostInternetDocumentDto ToNewPostInternetDocumentDto(this NewPostInternetDocument internetDocument)
    {
        return new NewPostInternetDocumentDto
        {
            Ref = internetDocument.Ref,
            CostOnSite = internetDocument.CostOnSite,
            EstimatedDeliveryDate = internetDocument.EstimatedDeliveryDate,
            IntDocNumber = internetDocument.IntDocNumber,
            TypeDocument = internetDocument.TypeDocument,
            OrderId = internetDocument.OrderId,
        };
    }
}
