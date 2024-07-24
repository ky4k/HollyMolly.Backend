using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;

namespace HM.BLL.Interfaces;

public interface INewPostService
{
    Task<OperationResult<IEnumerable<NewPostWarehouse>>> GetWarehousesAync(
        string? CityName,string? WarehouseId,string? FindByString, string? CityRef,string? Page, string? Limit,
        string? Language, string? TypeOfWarehouseRef, CancellationToken cancellationToken);

}
