using HM.DAL.Constants;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity;

namespace WebAPI.IntegrationTests.TestHelpers;

public static class SeedDefaultEntities
{
    public static async Task SeedAsync(HmDbContext? context, UserManager<User>? userManager, RoleManager<Role>? roleManager)
    {
        if (roleManager != null)
        {
            await roleManager.CreateAsync(new Role(DefaultRoles.Administrator));
            await roleManager.CreateAsync(new Role(DefaultRoles.Manager));
            await roleManager.CreateAsync(new Role(DefaultRoles.User));
        }
        if (userManager != null)
        {
            foreach (User user in Users)
            {
                await userManager.CreateAsync(user);
                await userManager.AddPasswordAsync(user, "password");
                await userManager.AddToRoleAsync(user, DefaultRoles.User);
            }
            foreach (User admin in Admins)
            {
                await userManager.CreateAsync(admin);
                await userManager.AddPasswordAsync(admin, "password");
                await userManager.AddToRoleAsync(admin, DefaultRoles.User);
                await userManager.AddToRoleAsync(admin, DefaultRoles.Administrator);
            }
        }
        if (context != null)
        {
            await context.CategoryGroups.AddRangeAsync(CategoryGroups);
            await context.Categories.AddRangeAsync(Categories);
            await context.Products.AddRangeAsync(Products);
            await context.Orders.AddRangeAsync(Orders);
            await context.WishLists.AddRangeAsync(WishLists);
            await context.NewsSubscriptions.AddRangeAsync(NewsSubscriptions);
            await context.EmailLogs.AddRangeAsync(EmailLogs);
            await context.SaveChangesAsync();
        }
    }
    private static List<User> Users =>
    [
        new()
        {
            Id = "1",
            Email = "user1@example.com",
            UserName = "user1@example.com",
            FirstName = "First",
            LastName = "User"
        },
        new()
        {
            Id = "2",
            UserName = "user2@example.com",
            Email = "user2@example.com",
            FirstName = "Second",
            LastName = "User"
        },
        new()
        {
            Id = "3",
            UserName = "user3@example.com",
            Email = "user3@example.com",
            FirstName = "Third",
            LastName = "User"
        },
        new()
        {
            Id = "4",
            UserName = "user4@example.com",
            Email = "user4@example.com",
            FirstName = "Fourth",
            LastName = "User"
        },
        new()
        {
            Id = "5",
            UserName = "user5@example.com",
            Email = "user5@example.com",
            FirstName = "Fifth",
            LastName = "User"
        },
    ];
    private static List<User> Admins =>
    [
        new()
        {
            Id = "51",
            UserName = "admin1@example.com",
            Email = "admin1@example.com",
            FirstName = "Default",
            LastName = "Administrator"
        }
    ];

    private static List<CategoryGroup> CategoryGroups =>
    [
        new()
        {
            Id = 1,
            Name = "Category group 1"
        },
        new()
        {
            Id = 2,
            Name = "Category group 2",
            ImageLink = "links/categoryGroup2",
            ImageFilePath = "wwwroot/links/categoryGroup2"
        },
        new()
        {
            Id = 3,
            Name = "Category group 3"
        }
    ];
    private static List<Category> Categories =>
    [
        new()
        {
            Id = 1,
            Name = "Category 1",
            CategoryGroupId = 1
        },
        new()
        {
            Id = 2,
            Name = "Category 2",
            CategoryGroupId = 1
        },
        new()
        {
            Id = 3,
            Name = "Category 3",
            CategoryGroupId = 2
        },
        new()
        {
            Id = 4,
            Name = "Category 4",
            CategoryGroupId = 2,
            ImageLink = "links/category4",
            ImageFilePath = "wwwroot/links/category4"
        },
        new()
        {
            Id = 5,
            Name = "Category 5",
            CategoryGroupId = 2
        }
    ];

    private static List<Product> Products =>
    [
        new()
        {
            Id = 1,
            CategoryId = 1,
            Name = "Product 1",
            Description = "Description 1",
            Rating = 0.5m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 1,
                    StockQuantity = 100,
                    Price = 60,
                    AbsoluteDiscount = 10
                },
                new()
                {
                    Id = 2,
                    StockQuantity = 100,
                    Price = 60,
                    PercentageDiscount = 10,
                    IsNewCollection = true
                }
            ],
            Feedbacks =
            [
                new()
                {
                    Id = 1,
                    AuthorName = "Author",
                    Rating = 1,
                    Review = "Review"
                },
                new()
                {
                    Id = 2,
                    AuthorName = "Author",
                    Rating = 1,
                    Review = "Review"
                }
            ],
            WishLists = [],
            ProductStatistics = []
        },
        new()
        {
            Id = 2,
            CategoryId = 1,
            Name = "Product 2",
            Description = "Description 2",
            Rating = 0.6m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 3,
                    StockQuantity = 100,
                    Price = 55,
                    AbsoluteDiscount = 0
                },
                new()
                {
                    Id = 4,
                    StockQuantity = 100,
                    Price = 70,
                    PercentageDiscount = 10,
                    IsNewCollection = true
                }
            ],
            Feedbacks =
            [
                new()
                {
                    Id = 3,
                    AuthorName = "Author",
                    Rating = 1,
                    Review = "Review"
                }
            ],
            WishLists = [],
            ProductStatistics = []
        },
        new()
        {
            Id = 3,
            CategoryId = 2,
            Name = "Product 3",
            Description = "Description 3",
            Rating = 0.7m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 5,
                    StockQuantity = 100,
                    Price = 80,
                    AbsoluteDiscount = 10
                },
                new()
                {
                    Id = 6,
                    StockQuantity = 100,
                    Price = 80,
                    PercentageDiscount = 10
                }
            ],
            Feedbacks =
            [
                new() {
                    Id = 4,
                    AuthorName = "Author",
                    Created = new DateTimeOffset(2024, 4, 22, 20, 0 , 0, TimeSpan.Zero),
                    Rating = 1,
                    Review = "Review"
                }
            ],
            WishLists = [],
            ProductStatistics = []
        },
        new()
        {
            Id = 4,
            CategoryId = 2,
            Name = "Product 4",
            Description = "Description 4",
            Rating = 0.8m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 7,
                    StockQuantity = 100,
                    Price = 90,
                    AbsoluteDiscount = 10
                },
                new()
                {
                    Id = 8,
                    StockQuantity = 100,
                    Price = 90,
                    PercentageDiscount = 10
                }
            ],
            Feedbacks = [],
            WishLists = [],
            ProductStatistics =
            [
                new()
                {
                    Id = 4,
                    Year = 2024,
                    Month = 4,
                    Day = 1,
                    NumberViews = 25
                }
            ]
        },
        new()
        {
            Id = 5,
            CategoryId = 3,
            Name = "Product 5",
            Description = "Description 5",
            Rating = -0.5m,
            TimesRated = 4,
            ProductInstances =
            [
                new()
                {
                    Id = 9,
                    StockQuantity = 100,
                    Price = 100,
                    AbsoluteDiscount = 10,
                    Images =
                    [
                        new()
                        {
                            Id = 1,
                            Position = 1,
                            Link = "test/link/name1",
                            FilePath = "test/path/name1"
                        },
                        new()
                        {
                            Id = 2,
                            Position = 2,
                            Link = "test/link/name1",
                            FilePath = "test/path/name1"
                        },
                        new()
                        {
                            Id = 3,
                            Position = 3,
                            Link = "test/link/name1",
                            FilePath = "test/path/name1"
                        }
                    ]
                },
                new()
                {
                    Id = 10,
                    StockQuantity = 100,
                    Price = 100,
                    PercentageDiscount = 10,
                    IsNewCollection = true
                }
            ],
            Feedbacks =
            [
                new()
                {
                    Id = 5,
                    AuthorName = "Author",
                    Rating = 1,
                    Review = "Review"
                }
            ],
            WishLists = [],
            ProductStatistics =
            [
                new()
                {
                    Id = 5,
                    Year = 2024,
                    Month = 4,
                    Day = 1,
                    NumberViews = 20
                }
            ]
        },
        new()
        {
            Id = 6,
            CategoryId = 3,
            Name = "Product 6",
            Description = "Description 6",
            Rating = -0.6m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 11,
                    StockQuantity = 100,
                    Price = 110,
                    AbsoluteDiscount = 10
                },
                new()
                {
                    Id = 12,
                    StockQuantity = 100,
                    Price = 110,
                    PercentageDiscount = 10,
                    IsNewCollection = true
                }
            ],
            Feedbacks = [],
            WishLists = [],
            ProductStatistics =
            [
                new()
                {
                    Id = 6,
                    Year = 2024,
                    Month = 4,
                    Day = 1,
                    NumberViews = 15
                }
            ]
        },
        new()
        {
            Id = 7,
            CategoryId = 4,
            Name = "Product 7",
            Description = "Description 7",
            Rating = -0.7m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 13,
                    StockQuantity = 100,
                    Price = 120,
                    AbsoluteDiscount = 10
                },
                new()
                {
                    Id = 14,
                    StockQuantity = 100,
                    Price = 130,
                    PercentageDiscount = 10
                }
            ],
            Feedbacks = [],
            WishLists = [],
            ProductStatistics = []
        },
        new()
        {
            Id = 8,
            CategoryId = 4,
            Name = "Product 8",
            Description = "Description 8",
            Rating = -0.8m,
            TimesRated = 10,
            ProductInstances =
            [
                new()
                {
                    Id = 15,
                    StockQuantity = 100,
                    Price = 130,
                    AbsoluteDiscount = 10
                },
                new()
                {
                    Id = 16,
                    StockQuantity = 100,
                    Price = 120,
                    PercentageDiscount = 10
                }
            ],
            Feedbacks = [],
            WishLists = [],
            ProductStatistics = []
        }
    ];

    private static List<Order> Orders =>
    [
        new()
        {
            Id = 1,
            Customer = new CustomerInfo()
            {
                Id = 1,
                Email = "user1@example.com",
                FirstName = "FirstName",
                LastName = "LastName",
                City = "City",
                DeliveryAddress = "Address",
                PhoneNumber = "1234567890"
            },
            UserId = "1",
            Status = "Created",
            OrderRecords =
            [
                new()
                {
                    Id = 1,
                    ProductInstanceId = 1,
                    Quantity = 1,
                    Price = 100,
                    ProductName = "Test product"
                }
            ]
        },
        new()
        {
            Id = 2,
            Customer = new CustomerInfo()
            {
                Id = 2,
                Email = "user2@example.com",
                FirstName = "FirstName",
                LastName = "LastName",
                City = "City",
                DeliveryAddress = "Address",
                PhoneNumber = "1234567890"
            },
            UserId = "2",
            Status = "Created",
            OrderRecords =
            [
                new()
                {
                    Id = 2,
                    ProductInstanceId = 1,
                    Quantity = 1,
                    Price = 100,
                    ProductName = "Test product"
                }
            ]
        },
        new()
        {
            Id = 3,
            Customer = new CustomerInfo()
            {
                Id = 3,
                Email = "user3@example.com",
                FirstName = "FirstName",
                LastName = "LastName",
                City = "City",
                DeliveryAddress = "Address",
                PhoneNumber = "1234567890"
            },
            UserId = "3",
            Status = "Created",
            OrderRecords =
            [
                new()
                {
                    Id = 3,
                    ProductInstanceId = 1,
                    Quantity = 1,
                    Price = 100,
                    ProductName = "Test product"
                }
            ]
        },
    ];
    private static List<WishList> WishLists =>
    [
        new()
        {
            Id = 1,
            UserId = "1"
        },
        new()
        {
            Id = 2,
            UserId = "2"
        }
    ];
    private static List<NewsSubscription> NewsSubscriptions =>
    [
        new()
        {
            Id = 1,
            Email = "user1@example.com",
            RemoveToken = "remove1"
        },
        new()
        {
            Id = 2,
            Email = "user2@example.com",
            RemoveToken = "remove2"
        }
    ];
    private static List<EmailLog> EmailLogs =>
    [
        new()
        {
            Id = 1,
            Subject = "Subject 1",
            RecipientEmail = "user1@example.com",
            SendAt = new DateTime(2024, 4, 1, 12, 0, 0, DateTimeKind.Utc)
        },
        new()
        {
            Id = 2,
            Subject = "Subject 2",
            RecipientEmail = "user2@example.com",
            SendAt = new DateTime(2024, 4, 2, 12, 0, 0, DateTimeKind.Utc)
        }
    ];
}
