using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;

namespace WebAPI.IntegrationTests.Mocks;

public class MockNewPostService : INewPostCityesService
{
    public Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CheckIfCityIsValidAsync(string city, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<OperationResult<IEnumerable<NewPostCity>>> GetCitiesAsync(string? name, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult<IEnumerable<NewPostCity>>(true, Array.Empty<NewPostCity>()));
    }

    public Task<OperationResult<IEnumerable<NewPostWarehouse>>> GetWarehousesAsync(string? warehouse, string koatuu, int page, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult<IEnumerable<NewPostWarehouse>>(true, Array.Empty<NewPostWarehouse>()));
    }
}
