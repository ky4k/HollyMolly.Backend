using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.WishLists;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services
{
    public class WishListService(HmDbContext context,
        ILogger<OrderService> logger) : IWishListService
    {
        public async Task<WishListDto?> GetWishListAsync(string userId, CancellationToken cancellationToken)
        {
            var wishList = await context.WishLists
                .Include(wl => wl.Products)
                .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);
            return wishList?.ToWishListDto();
        }

        public async Task<OperationResult<WishListDto>> AddProductToWishListAsync(string userId, int productId, CancellationToken cancellationToken)
        {
            var existingWishList = await context.WishLists
                .Include(wl => wl.Products)
                .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);

            if (existingWishList == null)
            {
                existingWishList = new WishList { UserId = userId };
                await context.WishLists.AddAsync(existingWishList, cancellationToken);
            }

            if (existingWishList.Products.Exists(p => p.Id == productId))
            {
                return new OperationResult<WishListDto>(false, "Product already exists in the wish list.");
            }

            var product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

            if (product == null)
            {
                return new OperationResult<WishListDto>(false, "Product not found.");
            }

            existingWishList.Products.Add(product);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return new OperationResult<WishListDto>(true, existingWishList.ToWishListDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while adding a product to the wish list.");
                return new OperationResult<WishListDto>(false, "Failed to add product to the wish list.");
            }
        }

        public async Task<OperationResult<WishListDto>> RemoveProductFromWishListAsync(string userId, int productId, CancellationToken cancellationToken)
        {
            var wishList = await context.WishLists
                .Include(wl => wl.Products)
                .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);

            if (wishList == null)
            {
                return new OperationResult<WishListDto>(false, "Wish list not found.");
            }

            var productToRemove = wishList.Products.Find(p => p.Id == productId);

            if (productToRemove == null)
            {
                return new OperationResult<WishListDto>(false, "Product not found in the wish list.");
            }

            wishList.Products.Remove(productToRemove);

            try
            {
                await context.SaveChangesAsync(cancellationToken);
                return new OperationResult<WishListDto>(true, wishList.ToWishListDto());
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "An error occurred while removing a product from the wish list.");
                return new OperationResult<WishListDto>(false, "Failed to remove product from the wish list.");
            }
        }
    }
}
