using HM.BLL.Interfaces;
using HM.BLL.Models;
using HM.DAL.Constants;
using HM.DAL.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HM.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WishListController(
        IWishListService wishListService,
        IStatisticsService statisticsService
        ) : ControllerBase
    {
        /// <summary>
        /// Allows administrators to retrieve the wish list for the specified user.
        /// </summary>
        /// <param name="userId">The ID of the user.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <response code="200">Returns the wish list for the specified user.</response>
        /// <response code="401">Indicates that the user is unauthenticated.</response>
        /// <response code="403">Indicates that the user has no permission to the action.</response>
        /// <response code="404">Indicates that the user has no wishList.</response>
        [Authorize(Roles = DefaultRoles.Administrator)]
        [HttpGet("{userId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WishListDto>> GetWishList(string userId, CancellationToken cancellationToken)
        {
            var wishList = await wishListService.GetWishListAsync(userId, cancellationToken);
            if (wishList == null)
            {
                return NotFound();
            }
            return Ok(wishList);
        }

        /// <summary>
        /// Allows a registered user to retrieve their with list.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <response code="200">Returns the user's wish list.</response>
        /// <response code="401">Indicates that the user is unauthenticated.</response>
        /// <response code="404">Indicates that the user has no wishList.</response>
        [Authorize]
        [HttpGet("myWishList")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<WishListDto>> GetMyWishList(CancellationToken cancellationToken)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var wishList = await wishListService.GetWishListAsync(userId, cancellationToken);
            if (wishList == null)
            {
                return NotFound();
            }
            return Ok(wishList);
        }

        /// <summary>
        /// Allows a registered user to add a product to their wish list.
        /// </summary>
        /// <param name="productId">The ID of the product to add.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <response code="200">Indicates that product was added and returns the user's wish list.</response>
        /// <response code="400">Indicates that the product was not added to the user's wishList
        /// and return the error message.</response>
        /// <response code="401">Indicates that the user is unauthenticated.</response>
        [Authorize]
        [HttpPost("myWishList/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WishListDto>> AddProductToWishList(int productId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await wishListService.AddProductToWishListAsync(userId, productId, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest(result.Message);
            }
            await statisticsService.AddToProductNumberWishlistAdditionsAsync(productId);
            return Ok(result.Payload);
        }

        /// <summary>
        /// Allows a registered user to remove product from their wish list.
        /// </summary>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <response code="200">Indicates that product was removed and returns the user's wish list.</response>
        /// <response code="400">Indicates that the product was not removed from the user's wishList
        /// and return the error message.</response>
        /// <response code="401">Indicates that the user is unauthenticated.</response>
        [Authorize]
        [HttpDelete("myWishList/{productId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<WishListDto>> RemoveProductFromWishList(int productId, CancellationToken cancellationToken)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized();
            }
            var result = await wishListService.RemoveProductFromWishListAsync(userId, productId, cancellationToken);
            if (!result.Succeeded)
            {
                return BadRequest(result.Message);
            }
            return Ok(result.Payload);
        }
    }
}
