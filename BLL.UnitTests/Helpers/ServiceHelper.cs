using HM.DAL.Data;
using Microsoft.EntityFrameworkCore;

namespace HM.BLL.UnitTests.Helpers;

public static class ServiceHelper
{
    public static DbContextOptions<HmDbContext> GetTestDbContextOptions()
    {
        return new DbContextOptionsBuilder<HmDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
    }
    public static HmDbContext GetTestDbContext()
    {
        return new HmDbContext(GetTestDbContextOptions());
    }
}
