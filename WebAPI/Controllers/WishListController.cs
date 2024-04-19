using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.BLL.Services;
using Microsoft.AspNetCore.Mvc;

namespace HM.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishListController(IWishListService wishListService) : ControllerBase
    {
        /// <summary>
        /// Retrieves the wish list for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The wish list for the specified user.</returns>
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<WishListDto>> GetWishList(int userId, CancellationToken cancellationToken)
        {
            var wishList = await wishListService.GetWishListAsync(userId, cancellationToken);
            if (wishList == null)
            {
                return NotFound();
            }
            return Ok(wishList);
        }
        /// <summary>
        /// Adds a product to the wish list.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to add.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An operation result indicating success or failure.</returns>
        [HttpPost("{userId}/products/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OperationResult<WishListDto>>> AddProductToWishList(int userId, int productId, CancellationToken cancellationToken)
        {
            var result = await wishListService.AddProductToWishListAsync(userId, productId, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
        /// <summary>
        /// Removes a product from the wish list.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>An operation result indicating success or failure.</returns>
        [HttpDelete("{userId}/products/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<OperationResult<WishListDto>>> RemoveProductFromWishList(int userId, int productId, CancellationToken cancellationToken)
        {
            var result = await wishListService.RemoveProductFromWishListAsync(userId, productId, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}
