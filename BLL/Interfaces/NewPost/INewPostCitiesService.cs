using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;

namespace HM.BLL.Interfaces.NewPost;

public interface INewPostCitiesService
{
    Task<IEnumerable<NewPostCities>> GetCitiesAsync(
        string? FindByString, string? Ref, string? Page, string? Limit, CancellationToken cancellationToken);
    Task<IEnumerable<NewPostWarehouse>> GetWarehousesAync(
        string? CityName, string? WarehouseId, string? FindByString, string? CityRef, string? Page, string? Limit,
        string? Language, string? TypeOfWarehouseRef, CancellationToken cancellationToken);
    Task<IEnumerable<NewPostStreets>> GetStreetsAync(string CityRef, string FindByString, string? Page, 
        string? Limit, CancellationToken cancellationToken);
    Task<OperationResult> UpadateCounterPartyAdressAsync(string CounterPartyRef,string AdressRef, string StreetRef, string? BuildingNumber, CancellationToken cancellationToken);
    Task<bool> CheckIfCityIsValidAsync(string city, CancellationToken cancellationToken);
    Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken);

}
