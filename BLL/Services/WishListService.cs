using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HM.BLL.Services
{
    public class WishListService(HmDbContext context,
        ILogger<OrderService> logger) : IWishListService
    {
        public async Task<WishListDto?> GetWishListAsync(int userId, CancellationToken cancellationToken)
        {
            var wishList = await context.WishLists
                .Include(wl => wl.Products)
                .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);
            if (wishList == null)
            {
                return null;
            }
            return wishList?.ToWishListDto();
        }

        public async Task<OperationResult<WishListDto>> AddProductToWishListAsync(int userId, int productId, CancellationToken cancellationToken)
        {
            var existingWishList = await context.WishLists
                .Include(wl => wl.Products)
                .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);

            if (existingWishList == null)
            {
                existingWishList = new WishList { UserId = userId };
                context.WishLists.Add(existingWishList);
            }

            if (existingWishList.Products.Any(p => p.Id == productId))
            {
                return new OperationResult<WishListDto>(false, "Product already exists in the wish list.");
            }

            var product = await context.Products.FindAsync(productId);

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

        public async Task<OperationResult<WishListDto>> RemoveProductFromWishListAsync(int userId, int productId, CancellationToken cancellationToken)
        {
            var wishList = await context.WishLists
                .Include(wl => wl.Products)
                .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);

            if (wishList == null)
            {
                return new OperationResult<WishListDto>(false, "Wish list not found.");
            }

            var productToRemove = wishList.Products.FirstOrDefault(p => p.Id == productId);

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
