using HM.DAL.Entities;

namespace HM.BLL.Interfaces
{
    public interface IWishListService
    {
        WishList CreateWishList(string userId);
        void AddProductToWishList(int wishListId, Product product);
        void RemoveProductFromWishList(int wishListId, int productId);
    }
}
