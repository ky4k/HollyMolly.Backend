using HM.BLL.Models;
using HM.DAL.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Interfaces
{
    public interface IWishListService
    {
        Task<WishListDto?> GetWishListAsync(int userId, CancellationToken cancellationToken);
        Task<OperationResult<WishListDto>> AddProductToWishListAsync(int userId, int productId, CancellationToken cancellationToken);
        Task<OperationResult<WishListDto>> RemoveProductFromWishListAsync(int userId, int productId, CancellationToken cancellationToken);
    }
}
