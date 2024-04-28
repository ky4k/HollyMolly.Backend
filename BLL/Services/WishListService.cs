using HM.BLL.Extensions;
using HM.BLL.Interfaces;
using HM.BLL.Models.Common;
using HM.BLL.Models.WishLists;
using HM.DAL.Data;
using HM.DAL.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HM.BLL.Services;

public class WishListService(
    HmDbContext context,
    ILogger<WishListService> logger
    ) : IWishListService
{
    public async Task<WishListDto?> GetWishListAsync(string userId, CancellationToken cancellationToken)
    {
        WishList? wishList = await context.WishLists
            .Include(wl => wl.Products)
            .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);
        return wishList?.ToWishListDto();
    }

    public async Task<OperationResult<WishListDto>> AddProductToWishListAsync(
        string userId, int productId, CancellationToken cancellationToken)
    {
        WishList? existingWishList = await context.WishLists
            .Include(wl => wl.Products)
            .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);

        if (existingWishList == null)
        {
            existingWishList = new WishList { UserId = userId };
            await context.WishLists.AddAsync(existingWishList, cancellationToken);
        }

        if (existingWishList.Products.Exists(p => p.Id == productId))
        {
            return new OperationResult<WishListDto>(false, "Product already exists in the wish list.",
                existingWishList.ToWishListDto());
        }

        Product? product = await context.Products.FirstOrDefaultAsync(p => p.Id == productId, cancellationToken);

        if (product == null)
        {
            return new OperationResult<WishListDto>(false, "Product not found.");
        }
        try
        {
            existingWishList.Products.Add(product);
            await context.SaveChangesAsync(cancellationToken);
            return new OperationResult<WishListDto>(true, existingWishList.ToWishListDto());
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while adding a product to the wish list.");
            return new OperationResult<WishListDto>(false, "Failed to add product to the wish list.");
        }
    }

    public async Task<OperationResult<WishListDto>> RemoveProductFromWishListAsync(
        string userId, int productId, CancellationToken cancellationToken)
    {
        WishList? wishList = await context.WishLists
            .Include(wl => wl.Products)
            .FirstOrDefaultAsync(wl => wl.UserId == userId, cancellationToken);

        if (wishList == null)
        {
            return new OperationResult<WishListDto>(false, "Wish list not found.");
        }

        Product? productToRemove = wishList.Products.Find(p => p.Id == productId);

        if (productToRemove == null)
        {
            return new OperationResult<WishListDto>(false, "Product not found in the wish list.",
                wishList.ToWishListDto());
        }
        try
        {
            wishList.Products.Remove(productToRemove);
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
