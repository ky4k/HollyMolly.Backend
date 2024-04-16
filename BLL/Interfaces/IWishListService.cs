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
        WishList CreateWishList(string userId);
        void AddProductToWishList(int wishListId, Product product);
        void RemoveProductFromWishList(int wishListId, int productId);
    }
}
