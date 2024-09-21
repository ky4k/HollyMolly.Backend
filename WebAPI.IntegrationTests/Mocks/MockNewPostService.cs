using HM.BLL.Interfaces.NewPost;
using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;

namespace WebAPI.IntegrationTests.Mocks;

public class MockNewPostService : INewPostCitiesService
{
    public Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<bool> CheckIfCityIsValidAsync(string city, CancellationToken cancellationToken)
    {
        return Task.FromResult(true);
    }

    public Task<IEnumerable<NewPostCities>> GetCitiesAsync(string? FindByString, string? Ref, string? Page, string? Limit, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<NewPostCities>());
    }

    public Task<IEnumerable<NewPostStreets>> GetStreetsAync(string CityRef, string FindByString, string? Page, string? Limit, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<NewPostStreets>());
    }

    public Task<OperationResult<IEnumerable<NewPostWarehouse>>> GetWarehousesAsync(string? warehouse, string koatuu, int page, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult<IEnumerable<NewPostWarehouse>>(true, Array.Empty<NewPostWarehouse>()));
    }

    public Task<IEnumerable<NewPostWarehouse>> GetWarehousesAync(string? CityName, string? WarehouseId, string? FindByString, string? CityRef, string? Page, string? Limit, string? Language, string? TypeOfWarehouseRef, CancellationToken cancellationToken)
    {
        return Task.FromResult(Enumerable.Empty<NewPostWarehouse>());
    }

    public Task<OperationResult> UpadateCounterPartyAdressAsync(string CounterPartyRef, string AdressRef, string StreetRef, string? BuildingNumber, CancellationToken cancellationToken)
    {
        return Task.FromResult(new OperationResult(true));
    }
}
