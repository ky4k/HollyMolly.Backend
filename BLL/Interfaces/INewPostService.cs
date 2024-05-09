using HM.BLL.Models.Common;
using HM.BLL.Models.NewPost;

namespace HM.BLL.Interfaces;

public interface INewPostService
{
    Task<OperationResult<IEnumerable<NewPostCity>>> GetCitiesAsync(
        string? name, int page, CancellationToken cancellationToken);

    Task<OperationResult<IEnumerable<NewPostWarehouse>>> GetWarehousesAsync(
        string? koatuu, int page, CancellationToken cancellationToken);

    Task<bool> CheckIfAddressIsValidAsync(string city, string address, CancellationToken cancellationToken);
}
