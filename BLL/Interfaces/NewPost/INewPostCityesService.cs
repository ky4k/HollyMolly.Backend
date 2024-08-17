using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;

namespace HM.BLL.Interfaces.NewPost;

public interface INewPostCityesService
{
    Task<OperationResult<IEnumerable<NewPostCities>>> GetCitiesAsync(
        string? FindByString, string? Ref, string? Page, string? Limit, CancellationToken cancellationToken);
    Task<OperationResult<IEnumerable<NewPostWarehouse>>> GetWarehousesAync(
        string? CityName, string? WarehouseId, string? FindByString, string? CityRef, string? Page, string? Limit,
        string? Language, string? TypeOfWarehouseRef, CancellationToken cancellationToken);
    Task<bool> CheckIfCityIsValidAsync(string city, CancellationToken cancellationToken);
    Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken);

}
