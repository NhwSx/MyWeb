using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;
using MyWebShop.Services;

namespace MyWebShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly UserManager<ApplicationUser> _userManager;

        public CartController(ICartService cartService, UserManager<ApplicationUser> userManager)
        {
            _cartService = cartService;
            _userManager = userManager;
        }

        private string GetSessionId()
        {
            var cookie = Request.Cookies["CartSessionId"];
            if (string.IsNullOrEmpty(cookie))
            {
                cookie = Guid.NewGuid().ToString();
                Response.Cookies.Append("CartSessionId", cookie, new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddDays(30),
                    HttpOnly = true,
                    IsEssential = true
                });
            }
            return cookie;
        }

        private string? GetUserId()
        {
            return _userManager.GetUserId(User);
        }

        // GET: /Cart
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var sessionId = GetSessionId();
            var model = await _cartService.GetCartAsync(userId, sessionId);
            return View(model);
        }

        // POST: /Cart/AddToCart
        [HttpPost]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            if (request == null || request.ProductId <= 0 || request.Quantity <= 0)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var userId = GetUserId();
            var sessionId = GetSessionId();

            var (success, message, count) = await _cartService.AddToCartAsync(userId, sessionId, request.ProductId, request.Quantity);

            return Json(new
            {
                success = success,
                message = message,
                cartCount = count
            });
        }

        // POST: /Cart/UpdateQuantity
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity([FromBody] UpdateCartItemRequest request)
        {
            if (request == null)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            var userId = GetUserId();
            var sessionId = GetSessionId();

            var (success, message, lineTotal, subTotal, grandTotal, cartCount) =
                await _cartService.UpdateQuantityAsync(userId, sessionId, request.CartItemId, request.Quantity);

            return Json(new
            {
                success = success,
                message = message,
                lineTotal = lineTotal.ToString("#,##0") + "₫",
                subTotal = subTotal.ToString("#,##0") + "₫",
                grandTotal = grandTotal.ToString("#,##0") + "₫",
                cartCount = cartCount
            });
        }

        // POST: /Cart/RemoveItem
        [HttpPost]
        public async Task<IActionResult> RemoveItem([FromBody] int cartItemId)
        {
            var userId = GetUserId();
            var sessionId = GetSessionId();

            var (success, message, subTotal, grandTotal, cartCount) =
                await _cartService.RemoveItemAsync(userId, sessionId, cartItemId);

            return Json(new
            {
                success = success,
                message = message,
                subTotal = subTotal.ToString("#,##0") + "₫",
                grandTotal = grandTotal.ToString("#,##0") + "₫",
                cartCount = cartCount
            });
        }

        // POST: /Cart/ClearCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            var sessionId = GetSessionId();
            await _cartService.ClearCartAsync(userId, sessionId);
            TempData["SuccessMessage"] = "Đã làm trống giỏ hàng!";
            return RedirectToAction(nameof(Index));
        }

        // GET: /Cart/GetCartCount
        [HttpGet]
        public async Task<IActionResult> GetCartCount()
        {
            var userId = GetUserId();
            var sessionId = Request.Cookies["CartSessionId"];
            var count = await _cartService.GetCartCountAsync(userId, sessionId);
            return Json(new { count = count });
        }
    }
}
