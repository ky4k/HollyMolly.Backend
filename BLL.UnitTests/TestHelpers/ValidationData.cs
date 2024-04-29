using HM.BLL.Models.Categories;
using HM.BLL.Models.NewsSubscriptions;
using HM.BLL.Models.Orders;
using HM.BLL.Models.Products;
using HM.BLL.Models.Users;

namespace HM.BLL.UnitTests.TestHelpers;

public static class ValidationData
{
    private static readonly IEnumerable<string> _validEmails =
    [
        "1@gmail.com",
        "john.doe43@domainsample.co.uk",
        "1234567890123456789TEST-10_test+test_jo.john.doe43@domainsample.co.uk",
        "test@g.co",
        "john.doe43@domain1234567890-1234567890-1234567890sample.co.uk",
        "1234567890123456789TEST-10_test+test_jo.john.doe43@domain1234567890-1234567890-1234567890sample.co.uk",
    ];
    private static readonly IEnumerable<string> _invalidEmails =
    [
        "",
        "@gmail.com",
        "01234567890123456789TEST-10_test+test_jo.john.doe43@domainsample.co.uk",
        "test@g.c",
        "john.doe43@domain01234567890-1234567890-1234567890sample.co.uk",
        "testAgmail.com",
        "test@gmailcom",
        "example.1@!gmail.com",
        "exa!mple@gmail.com",
        "test@email@gmail.com",
        "invalid@format.",
        "invalid@f....."
    ];
    private static readonly IEnumerable<string> _validPasswords =
    [
        "abcdefgh",
        "abcdefghij123456789012345",
        "~`!@#$%^&*()_-+=",
        "{[}]|:;'<,>.?/",
        "~0-,>.?/",
        "1_Test78",
        "986757ab",
        "Lamb432ttq",
        "Qa9`!@#$%^&*()+={[}]|:;'<"
    ];
    private static readonly IEnumerable<string> _invalidPasswords =
    [
        "",
        "a",
        "ab",
        "abc",
        "abcd",
        "abcde",
        "abcdef",
        "abcdefg",
        "12345678",
        "1234567890",
        "abcdefghijklmnopqrstuvwxyz1234567890",
        "abcd1234\"",
        "abcd1234\\",
        "TestT12",
        "12345678901234567890zcbmad",
        "агLєпщЦ4"
    ];
    private static readonly IEnumerable<string> _validNames =
    [
        "А",
        "Ч",
        "Мирослав",
        "Тесттест",
        "Через-Дефіс",
        "З-Апостроф'ом`різного’типу",
        "Ім'я-Якеміститьрівноп'ятдесятсимволів-Івсієвалідні",
    ];
    private static readonly IEnumerable<string> _invalidNames =
    [
        "D",
        "Z",
        "БезДефісу",
        "Через-дефіс",
        "ВеликаЛітера",
        "Невал!дні-Символи",
        "Невалідні-Сим$оли",
        "Невалідні-Симв@ли",
        "Надтодовгеім'якористувача-Міститьлишеваліднісимволи",
        "змалоїлітери",
        "Два''апострофи",
        "Два--Дефіси",
        "Ком-'бінації",
        "Ком`'бінації",
        "Ком'-бінації",
    ];
    private static readonly IEnumerable<string> _validPhoneNumbers =
    [
        "0123456789",
        "+380123456789"
    ];
    private static readonly IEnumerable<string> _invalidPhoneNumbers =
    [
        "012345678",
        "01234567890",
        "+3801234567890",
        "+38012345678"
    ];
    private static readonly IEnumerable<DateOnly> _validDateOfBirth =
    [
        new DateOnly(1980, 1, 1),
        new DateOnly(1990, 1, 1),
        new DateOnly(2000, 1, 1),
        new DateOnly(2010, 1, 1),
        new DateOnly(2020, 1, 1),
    ];
    private static readonly IEnumerable<DateOnly> _invalidDateOfBirth =
    [
        new DateOnly(2030, 1, 1),
        new DateOnly(2040, 1, 1),
        new DateOnly(2050, 1, 1),
    ];
    private static readonly IEnumerable<string> _validCities =
    [
        "City",
        "Місто"
    ];
    private static readonly IEnumerable<string> _invalidCities =
    [

    ];
    private static readonly IEnumerable<string> _validDeliveryAddress =
    [
        "Address",
        "Адреса"
    ];
    private static readonly IEnumerable<string> _invalidDeliveryAddress =
    [

    ];

    private static readonly IEnumerable<string> _validCategoryNames =
    [
        "Name",
        "abc",
        "A b ",
        "Українські літери",
        "1234567890",
        "Combined різні 12345",
        "Exact twenty symbols"
    ];
    private static readonly IEnumerable<string> _invalidCategoryNames =
    [
        "",
        "1",
        "a",
        "ab",
        "  ",
        "               ",
        "Invalid symbols $#",
        "!@#$%^&*())_-+=",
        "This name is too long to be valid"
    ];
    private static readonly IEnumerable<string> _validProductNames =
    [
        "Fi ve",
        "T    e   n",
        "Fifty letters is the longest accepted product name",

    ];
    private static readonly IEnumerable<string> _invalidProductNames =
    [
        "",
        "Four",
        "Fifty one letters is tooo long for the product name"
    ];
    private static readonly IEnumerable<string> _validProductDescriptions =
    [
        "",
        "T",
        new string('a', 500)
    ];
    private static readonly IEnumerable<string> _invalidProductDescriptions =
    [
        new string('a', 501)
    ];
    private static readonly IEnumerable<string> _validFeedbacks =
    [
        "Allowed symbol !#$%&\"/?,.-_",
        "!#$%&\"/?,.-_();:'",
        "special characters ( ! # $ % & \"  / ? , . - _ )",
        "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯабвгґдеєжзиіїйклмнопрстуфхцчшщьюя   ABCDEFGHIJKLMNOPQRSTUVWXYZ    abcdefghijklmnopqrstuvwxyz",
        new string('a', 2000)
    ];
    private static readonly IEnumerable<string> _invalidFeedbacks =
    [
        "abc",
        "Invalid symbol | ",
        "Invalid symbol @",
        new string('a', 2001)
    ];
    private static readonly IEnumerable<int> _validRatings = [-1, 0, 1];
    private static readonly IEnumerable<int> _invalidRatings = [-100, -2, 2, 100];

    private static readonly IEnumerable<string> _validOrderStatuses =
    [
        "Created",
        "Payment Received",
        "Processing",
        "Shipped",
        "Delivered",
        "Cancelled"
    ];
    private static readonly IEnumerable<string> _invalidOrderStatuses =
    [
        "not a status"
    ];
    private static readonly IEnumerable<string> _validOrderNotes =
    [
        "",
        "!#$%&\"/?,.-_",
        "special characters ( ! # $ % & \"  / ? , . - _ )",
        "АБВГҐДЕЄЖЗИІЇЙКЛМНОПРСТУФХЦЧШЩЮЯабвгґдеєжзиіїйклмнопрстуфхцчшщьюя   ABCDEFGHIJKLMNOPQRSTUVWXYZ    abcdefghijklmnopqrstuvwxyz",
        new string('a', 500)
    ];
    private static readonly IEnumerable<string> _invalidOrderNotes =
    [
        "@ is not in the allowed characters",
        new string('a', 501)
    ];


    public static TheoryData<RegistrationRequest> ValidRegistrationRequests
        => new(GetValidRegistrationRequests());
    public static TheoryData<RegistrationRequest> InvalidRegistrationRequests
        => new(GetInvalidRegistrationRequests());
    public static TheoryData<ProfileUpdateDto> ValidProfileUpdates
        => new(GetValidProfileUpdateDtos());
    public static TheoryData<ProfileUpdateDto> InvalidProfileUpdates
        => new(GetInvalidProfileUpdateDtos());
    public static TheoryData<EmailUpdateDto> ValidEmailUpdates
        => new(_validEmails.Select(e => new EmailUpdateDto() { NewEmail = e }));
    public static TheoryData<EmailUpdateDto> InvalidEmailUpdates
        => new(_invalidEmails.Select(e => new EmailUpdateDto() { NewEmail = e }));
    public static TheoryData<ChangePasswordDto> ValidChangePasswordModels
        => new(_validPasswords.Select(p => new ChangePasswordDto() { OldPassword = "old", NewPassword = p }));
    public static TheoryData<ChangePasswordDto> InvalidChangePasswordModels
        => new(_invalidPasswords.Select(p => new ChangePasswordDto() { OldPassword = "old", NewPassword = p }));
    public static TheoryData<ResetPasswordDto> ValidResetPasswordModels
        => new(_validPasswords.Select(p => new ResetPasswordDto() { ResetToken = "old", NewPassword = p }));
    public static TheoryData<ResetPasswordDto> InvalidResetPasswordModels
        => new(_invalidPasswords.Select(p => new ResetPasswordDto() { ResetToken = "old", NewPassword = p }));
    public static TheoryData<CategoryGroupCreateDto> ValidCategoryGroups
        => new(_validCategoryNames.Select(c => new CategoryGroupCreateDto() { Name = c }));
    public static TheoryData<CategoryGroupCreateDto> InvalidCategoryGroups
        => new(_invalidCategoryNames.Select(c => new CategoryGroupCreateDto() { Name = c }));
    public static TheoryData<CategoryCreateDto> ValidCategories
        => new(_validCategoryNames.Select(c => new CategoryCreateDto() { CategoryName = c }));
    public static TheoryData<CategoryCreateDto> InvalidCategories
        => new(_invalidCategoryNames.Select(c => new CategoryCreateDto() { CategoryName = c }));
    public static TheoryData<CategoryUpdateDto> ValidCategoryUpdates
        => new(_validCategoryNames.Select(c => new CategoryUpdateDto() { CategoryName = c }));
    public static TheoryData<CategoryUpdateDto> InvalidCategoryUpdates
        => new(_invalidCategoryNames.Select(c => new CategoryUpdateDto() { CategoryName = c }));
    public static TheoryData<ProductCreateDto> ValidProductsCreate
        => new(GetValidProductsCreateDto());
    public static TheoryData<ProductCreateDto> InvalidProductsCreate
        => new(GetInvalidProductsCreateDto());
    public static TheoryData<ProductUpdateDto> ValidProductsUpdate
        => new(GetValidProductsUpdateDto());
    public static TheoryData<ProductUpdateDto> InvalidProductsUpdate
        => new(GetInvalidProductsUpdateDto());
    public static TheoryData<ProductInstanceCreateDto> ValidProductInstances
        => new(GetValidProductInstances());
    public static TheoryData<ProductInstanceCreateDto> InvalidProductInstances
        => new(GetInvalidProductInstances());
    public static TheoryData<ProductFeedbackCreateDto> ValidProductFeedbacks
        => new(GetValidProductFeedbacksCreateDto());
    public static TheoryData<ProductFeedbackCreateDto> InvalidProductFeedbacks
        => new(GetInvalidProductFeedbacksCreateDto());
    public static TheoryData<OrderCreateDto> ValidOrdersCreate => new(GetValidOrdersCreateDto());
    public static TheoryData<OrderCreateDto> InvalidOrdersCreate => new(GetInvalidOrdersCreateDto());
    public static TheoryData<OrderUpdateDto> ValidOrdersUpdate => new(GetValidOrdersUpdateDto());
    public static TheoryData<OrderUpdateDto> InvalidOrdersUpdate => new(GetInvalidOrdersUpdateDto());
    public static TheoryData<CustomerDto> ValidCustomers => new(GetValidCustomers());
    public static TheoryData<CustomerDto> InvalidCustomers => new(GetInvalidCustomers());
    public static TheoryData<NewsSubscriptionCreateDto> ValidNewsSubscriptions =>
        new(_validEmails.Select(e => new NewsSubscriptionCreateDto() { Email = e }));
    public static TheoryData<NewsSubscriptionCreateDto> InvalidNewsSubscriptions =>
        new(_invalidEmails.Select(e => new NewsSubscriptionCreateDto() { Email = e }));


    private static List<RegistrationRequest> GetValidRegistrationRequests()
    {
        List<RegistrationRequest> registrationRequests = [];
        foreach (string email in _validEmails)
        {
            registrationRequests.Add(new()
            {
                Email = email,
                Password = "validPassword"
            });
        }
        foreach (string password in _validPasswords)
        {
            registrationRequests.Add(new()
            {
                Email = "validEmail@example.com",
                Password = password
            });
        }
        return registrationRequests;
    }
    private static List<RegistrationRequest> GetInvalidRegistrationRequests()
    {
        List<RegistrationRequest> registrationRequests = [];
        foreach (string email in _invalidEmails)
        {
            registrationRequests.Add(new()
            {
                Email = email,
                Password = "validPassword"
            });
        }
        foreach (string password in _invalidPasswords)
        {
            registrationRequests.Add(new()
            {
                Email = "validEmail@example.com",
                Password = password
            });
        }
        return registrationRequests;
    }
    private static List<ProfileUpdateDto> GetValidProfileUpdateDtos()
    {
        List<ProfileUpdateDto> profiles = [];
        foreach (string name in _validNames)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.LastName = name;
            profile.LastName = name;
            profiles.Add(profile);
        }
        foreach (string phoneNumber in _validPhoneNumbers)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.PhoneNumber = phoneNumber;
            profiles.Add(profile);
        }
        foreach (string city in _validCities)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.City = city;
            profiles.Add(profile);
        }
        foreach (string address in _validDeliveryAddress)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.DeliveryAddress = address;
            profiles.Add(profile);
        }
        foreach (DateOnly dateOfBirth in _validDateOfBirth)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.DateOfBirth = dateOfBirth;
            profiles.Add(profile);
        }
        ProfileUpdateDto noDateOfBirth = GetProfileUpdatePrototype();
        noDateOfBirth.DateOfBirth = null;
        profiles.Add(noDateOfBirth);
        return profiles;
    }
    private static List<ProfileUpdateDto> GetInvalidProfileUpdateDtos()
    {
        List<ProfileUpdateDto> profiles = [];
        foreach (string name in _invalidNames)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.LastName = name;
            profile.LastName = name;
            profiles.Add(profile);
        }
        foreach (string phoneNumber in _invalidPhoneNumbers)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.PhoneNumber = phoneNumber;
            profiles.Add(profile);
        }
        foreach (string city in _invalidCities)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.City = city;
            profiles.Add(profile);
        }
        foreach (string address in _invalidDeliveryAddress)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.DeliveryAddress = address;
            profiles.Add(profile);
        }
        foreach (DateOnly dateOfBirth in _invalidDateOfBirth)
        {
            ProfileUpdateDto profile = GetProfileUpdatePrototype();
            profile.DateOfBirth = dateOfBirth;
            profiles.Add(profile);
        }
        return profiles;
    }
    private static ProfileUpdateDto GetProfileUpdatePrototype() => new()
    {
        FirstName = "Правильне",
        LastName = "Прізвище",
        PhoneNumber = "0123456789",
        DateOfBirth = null,
        City = "Місто",
        DeliveryAddress = "Адреса"
    };
    private static List<ProductCreateDto> GetValidProductsCreateDto()
    {
        List<ProductCreateDto> products = [];
        ProductInstanceCreateDto productInstance = GetValidProductInstances()[0];
        foreach (string name in _validProductNames)
        {
            products.Add(new ProductCreateDto()
            {
                Name = name,
                Description = "Valid description",
                ProductInstances = [productInstance]
            });
        }
        foreach (string description in _validProductDescriptions)
        {
            products.Add(new ProductCreateDto()
            {
                Name = "Valid name",
                Description = description,
                ProductInstances = [productInstance]
            });
        }
        return products;
    }
    private static List<ProductCreateDto> GetInvalidProductsCreateDto()
    {
        List<ProductCreateDto> products = [];
        ProductInstanceCreateDto productInstance = GetValidProductInstances()[0];
        foreach (string name in _invalidProductNames)
        {
            products.Add(new ProductCreateDto()
            {
                Name = name,
                Description = "Valid description",
                ProductInstances = [productInstance]
            });
        }
        foreach (string description in _invalidProductDescriptions)
        {
            products.Add(new ProductCreateDto()
            {
                Name = "Valid name",
                Description = description,
                ProductInstances = [productInstance]
            });
        }
        List<ProductInstanceCreateDto> invalidProductInstances = GetInvalidProductInstances();
        foreach (ProductInstanceCreateDto invalidInstance in invalidProductInstances)
        {
            products.Add(new ProductCreateDto()
            {
                Name = "Valid name",
                Description = "Valid description",
                ProductInstances = [invalidInstance]
            });
        }
        products.Add(new ProductCreateDto()
        {
            Name = "Valid name",
            Description = "Valid description",
            ProductInstances = invalidProductInstances
        });
        return products;
    }
    private static List<ProductUpdateDto> GetValidProductsUpdateDto()
    {
        List<ProductUpdateDto> products = [];
        foreach (string name in _validProductNames)
        {
            products.Add(new ProductUpdateDto()
            {
                Name = name,
                Description = "Valid description",
                CategoryId = 1
            });
        }
        foreach (string description in _validProductDescriptions)
        {
            products.Add(new ProductUpdateDto()
            {
                Name = "Valid name",
                Description = description,
                CategoryId = 1
            });
        }
        return products;
    }
    private static List<ProductUpdateDto> GetInvalidProductsUpdateDto()
    {
        List<ProductUpdateDto> products = [];
        foreach (string name in _invalidProductNames)
        {
            products.Add(new ProductUpdateDto()
            {
                Name = name,
                Description = "Valid description",
                CategoryId = 1
            });
        }
        foreach (string description in _invalidProductDescriptions)
        {
            products.Add(new ProductUpdateDto()
            {
                Name = "Valid name",
                Description = description,
                CategoryId = 1
            });
        }
        return products;
    }
    private static List<ProductInstanceCreateDto> GetValidProductInstances()
    {
        List<ProductInstanceCreateDto> productInstances = [];
        for (int i = 0; i < 8; i++)
        {
            productInstances.Add(GetProductInstancePrototype());
        }
        productInstances[0].Price = 0.1m;
        productInstances[1].Price = 50000;
        productInstances[2].Price = 999999;
        productInstances[3].StockQuantity = 1000;
        productInstances[4].StockQuantity = 0;
        productInstances[5].SKU = "Abc";
        productInstances[6].SKU = "12345678901234567890123456789012345678901234567890";
        productInstances[7].SKU = "Valid characters: #/-():_";
        return productInstances;
    }
    private static List<ProductInstanceCreateDto> GetInvalidProductInstances()
    {
        List<ProductInstanceCreateDto> productInstances = [];
        for (int i = 0; i < 9; i++)
        {
            productInstances.Add(GetProductInstancePrototype());
        }
        productInstances[0].Price = 0;
        productInstances[1].Price = -1;
        productInstances[2].Price = 1000000;
        productInstances[3].StockQuantity = -1;
        productInstances[4].SKU = "Ab";
        productInstances[5].SKU = "123456789012345678901234567890123456789012345678901";
        productInstances[6].SKU = "Invalid characters: \\";
        productInstances[7].SKU = "Invalid characters: \"";
        productInstances[8].SKU = "Invalid characters: @";
        return productInstances;
    }
    private static ProductInstanceCreateDto GetProductInstancePrototype() => new()
    {
        IsNewCollection = true,
        Price = 100,
        PercentageDiscount = 0,
        AbsoluteDiscount = 0,
        StockQuantity = 10
    };
    private static List<ProductFeedbackCreateDto> GetValidProductFeedbacksCreateDto()
    {
        List<ProductFeedbackCreateDto> feedbacks = [];
        foreach (string feedbackText in _validFeedbacks)
        {
            feedbacks.Add(new ProductFeedbackCreateDto()
            {
                AuthorName = "Author",
                Review = feedbackText,
                Rating = 1
            });
        }
        foreach (int rating in _validRatings)
        {
            feedbacks.Add(new ProductFeedbackCreateDto()
            {
                AuthorName = "Author",
                Review = "Valid feedback",
                Rating = rating
            });
        }
        return feedbacks;
    }
    private static List<ProductFeedbackCreateDto> GetInvalidProductFeedbacksCreateDto()
    {
        List<ProductFeedbackCreateDto> feedbacks = [];
        foreach (string feedbackText in _invalidFeedbacks)
        {
            feedbacks.Add(new ProductFeedbackCreateDto()
            {
                AuthorName = "Author",
                Review = feedbackText,
                Rating = 1
            });
        }
        foreach (int rating in _invalidRatings)
        {
            feedbacks.Add(new ProductFeedbackCreateDto()
            {
                AuthorName = "Author",
                Review = "Valid feedback",
                Rating = rating
            });
        }
        feedbacks.Add(new ProductFeedbackCreateDto()
        {
            AuthorName = "",
            Review = "Valid feedback",
            Rating = 1
        });
        return feedbacks;
    }

    private static List<OrderCreateDto> GetValidOrdersCreateDto()
    {
        List<OrderCreateDto> orders = [];
        foreach (var customer in GetValidCustomers())
        {
            orders.Add(new OrderCreateDto()
            {
                Customer = customer,
                OrderRecords =
                [
                    new()
                    {
                        ProductInstanceId = 1,
                        Quantity = 1
                    }
                ]
            });
        }
        return orders;
    }
    private static List<OrderCreateDto> GetInvalidOrdersCreateDto()
    {
        List<OrderCreateDto> orders = [];
        foreach (var customer in GetInvalidCustomers())
        {
            orders.Add(new OrderCreateDto()
            {
                Customer = customer,
                OrderRecords =
                [
                    new()
                    {
                        ProductInstanceId = 1,
                        Quantity = 1
                    }
                ]
            });
        }
        List<OrderRecordCreateDto> orderRecords = GetInvalidOrderRecords();
        orders.Add(new OrderCreateDto()
        {
            Customer = GetCustomerDtoPrototype(),
            OrderRecords = orderRecords
        });
        orders.Add(new OrderCreateDto()
        {
            Customer = GetCustomerDtoPrototype(),
            OrderRecords =
            [
                new()
                {
                    ProductInstanceId = 1,
                    Quantity = 1
                },
                orderRecords[0]
            ]
        });

        return orders;
    }
    private static List<OrderUpdateDto> GetValidOrdersUpdateDto()
    {
        List<OrderUpdateDto> orders = [];
        foreach (string status in _validOrderStatuses)
        {
            orders.Add(new OrderUpdateDto()
            {
                Status = status,
                Notes = "valid notes"
            });
        }
        foreach (string note in _validOrderNotes)
        {
            orders.Add(new OrderUpdateDto()
            {
                Status = "Created",
                Notes = note
            });
        }
        return orders;
    }
    private static List<OrderUpdateDto> GetInvalidOrdersUpdateDto()
    {
        List<OrderUpdateDto> orders = [];
        foreach (string status in _invalidOrderStatuses)
        {
            orders.Add(new OrderUpdateDto()
            {
                Status = status,
                Notes = "valid notes"
            });
        }
        foreach (string note in _invalidOrderNotes)
        {
            orders.Add(new OrderUpdateDto()
            {
                Status = "Created",
                Notes = note
            });
        }
        return orders;
    }
    private static List<OrderRecordCreateDto> GetInvalidOrderRecords()
    {
        return
        [
            new()
            {
                ProductInstanceId = 100,
                Quantity = -100
            },
            new()
            {
                ProductInstanceId = 200,
                Quantity = 0
            }
        ];
    }
    private static List<CustomerDto> GetValidCustomers()
    {
        List<CustomerDto> customers = [];
        foreach (string email in _validEmails)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.Email = email;
            customers.Add(customer);
        }
        foreach (string name in _validNames)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.FirstName = name;
            customer.LastName = name;
            customers.Add(customer);
        }
        foreach (string phoneNumber in _validPhoneNumbers)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.PhoneNumber = phoneNumber;
            customers.Add(customer);
        }
        foreach (string city in _validCities)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.City = city;
            customers.Add(customer);
        }
        foreach (string address in _validDeliveryAddress)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.DeliveryAddress = address;
            customers.Add(customer);
        }
        return customers;
    }
    private static List<CustomerDto> GetInvalidCustomers()
    {
        List<CustomerDto> customers = [];
        foreach (string email in _invalidEmails)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.Email = email;
            customers.Add(customer);
        }
        foreach (string name in _invalidNames)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.FirstName = name;
            customer.LastName = name;
            customers.Add(customer);
        }
        foreach (string phoneNumber in _invalidPhoneNumbers)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.PhoneNumber = phoneNumber;
            customers.Add(customer);
        }
        foreach (string city in _invalidCities)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.City = city;
            customers.Add(customer);
        }
        foreach (string address in _invalidDeliveryAddress)
        {
            CustomerDto customer = GetCustomerDtoPrototype();
            customer.DeliveryAddress = address;
            customers.Add(customer);
        }
        customers.AddRange(GetCustomerWithoutSomeProperties());
        return customers;
    }
    private static List<CustomerDto> GetCustomerWithoutSomeProperties()
    {
        List<CustomerDto> customers = [];
        for (int i = 0; i < 5; i++)
        {
            customers.Add(GetCustomerDtoPrototype());
        }
        customers[0].FirstName = "";
        customers[1].LastName = "";
        customers[2].PhoneNumber = "";
        customers[3].City = "";
        customers[4].DeliveryAddress = "";
        return customers;
    }
    private static CustomerDto GetCustomerDtoPrototype()
    {
        return new CustomerDto()
        {
            Email = "validEmail@example.com",
            FirstName = "Правильне",
            LastName = "Прізвище",
            PhoneNumber = "0123456789",
            City = "Місто",
            DeliveryAddress = "Адреса"
        };
    }
}
