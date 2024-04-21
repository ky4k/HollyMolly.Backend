using HM.BLL.Models.Common;
using HM.BLL.Models.WishLists;

namespace HM.BLL.Interfaces;

public interface IWishListService
{
    Task<WishListDto?> GetWishListAsync(string userId, CancellationToken cancellationToken);
    Task<OperationResult<WishListDto>> AddProductToWishListAsync(string userId, int productId, CancellationToken cancellationToken);
    Task<OperationResult<WishListDto>> RemoveProductFromWishListAsync(string userId, int productId, CancellationToken cancellationToken);
}
