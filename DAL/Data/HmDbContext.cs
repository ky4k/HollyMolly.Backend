using HM.DAL.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace HM.DAL.Data;

public class HmDbContext(DbContextOptions<HmDbContext> options)
    : IdentityDbContext<User, Role, string>(options)
{
    public DbSet<CategoryGroup> CategoryGroups => Set<CategoryGroup>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<WishList> WishLists => Set<WishList>();
    public DbSet<NewsSubscription> NewsSubscriptions => Set<NewsSubscription>();
    public DbSet<EmailLog> EmailLogs => Set<EmailLog>();
    public DbSet<ProductStatistics> ProductStatistics => Set<ProductStatistics>();
    public DbSet<Support> Supports => Set<Support>();
    public DbSet<TokenRecord> Tokens => Set<TokenRecord>();
    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
    }
}
