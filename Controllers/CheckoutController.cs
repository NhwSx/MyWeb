using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebShop.Data;
using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;
using MyWebShop.Services;

namespace MyWebShop.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CheckoutController(
            ICartService cartService,
            IOrderService orderService,
            UserManager<ApplicationUser> userManager,
            ApplicationDbContext context)
        {
            _cartService = cartService;
            _orderService = orderService;
            _userManager = userManager;
            _context = context;
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

        // GET: /Checkout
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var sessionId = GetSessionId();

            var cart = await _cartService.GetCartAsync(userId, sessionId);
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống. Vui lòng chọn sản phẩm trước khi thanh toán.";
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Items = cart.Items,
                SubTotal = cart.SubTotal,
                ShippingFee = cart.ShippingFee,
                TotalAmount = cart.GrandTotal
            };

            // Pre-fill user data if logged in
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.CustomerName = user.FullName;
                    model.CustomerEmail = user.Email ?? "";
                    model.CustomerPhone = user.PhoneNumber ?? "";

                    var defaultAddress = await _context.UserAddresses
                        .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

                    if (defaultAddress != null)
                    {
                        model.ProvinceCity = defaultAddress.ProvinceCity;
                        model.WardDistrict = defaultAddress.WardDistrict;
                        model.ShippingAddress = defaultAddress.StreetAddress;
                    }
                    else if (!string.IsNullOrEmpty(user.Address))
                    {
                        model.ShippingAddress = user.Address;
                    }
                }
            }

            return View(model);
        }

        // GET: /Checkout/BuyNow?productId=1&quantity=1
        public async Task<IActionResult> BuyNow(int productId, int quantity = 1)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                TempData["ErrorMessage"] = "Sản phẩm không khả dụng";
                return RedirectToAction("Index", "Home");
            }

            if (product.StockQuantity < quantity)
            {
                TempData["ErrorMessage"] = $"Sản phẩm chỉ còn {product.StockQuantity} trong kho";
                return RedirectToAction("Details", "Product", new { slug = product.Slug });
            }

            var itemDto = new CartItemDto
            {
                ProductId = product.Id,
                ProductName = product.Name,
                ProductSlug = product.Slug,
                ProductImageUrl = product.MainImageUrl,
                UnitPrice = product.Price,
                OriginalPrice = product.OriginalPrice,
                Quantity = quantity,
                StockQuantity = product.StockQuantity
            };

            decimal subTotal = product.Price * quantity;
            decimal shippingFee = subTotal >= 10000000 ? 0 : 30000;

            var model = new CheckoutViewModel
            {
                DirectProductId = product.Id,
                DirectQuantity = quantity,
                Items = new List<CartItemDto> { itemDto },
                SubTotal = subTotal,
                ShippingFee = shippingFee,
                TotalAmount = subTotal + shippingFee
            };

            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    model.CustomerName = user.FullName;
                    model.CustomerEmail = user.Email ?? "";
                    model.CustomerPhone = user.PhoneNumber ?? "";

                    var defaultAddress = await _context.UserAddresses
                        .FirstOrDefaultAsync(a => a.UserId == userId && a.IsDefault);

                    if (defaultAddress != null)
                    {
                        model.ProvinceCity = defaultAddress.ProvinceCity;
                        model.WardDistrict = defaultAddress.WardDistrict;
                        model.ShippingAddress = defaultAddress.StreetAddress;
                    }
                }
            }

            return View("Index", model);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var sessionId = GetSessionId();

            if (!ModelState.IsValid)
            {
                // Re-hydrate items
                if (model.DirectProductId.HasValue && model.DirectQuantity.HasValue)
                {
                    var prod = await _context.Products.FindAsync(model.DirectProductId.Value);
                    if (prod != null)
                    {
                        model.Items = new List<CartItemDto>
                        {
                            new CartItemDto
                            {
                                ProductId = prod.Id,
                                ProductName = prod.Name,
                                ProductSlug = prod.Slug,
                                ProductImageUrl = prod.MainImageUrl,
                                UnitPrice = prod.Price,
                                Quantity = model.DirectQuantity.Value
                            }
                        };
                        model.SubTotal = prod.Price * model.DirectQuantity.Value;
                        model.ShippingFee = model.SubTotal >= 10000000 ? 0 : 30000;
                        model.TotalAmount = model.SubTotal + model.ShippingFee;
                    }
                }
                else
                {
                    var cart = await _cartService.GetCartAsync(userId, sessionId);
                    model.Items = cart.Items;
                    model.SubTotal = cart.SubTotal;
                    model.ShippingFee = cart.ShippingFee;
                    model.TotalAmount = cart.GrandTotal;
                }
                return View("Index", model);
            }

            var (success, message, orderCode) = await _orderService.CreateOrderAsync(model, userId, sessionId);

            if (!success || string.IsNullOrEmpty(orderCode))
            {
                ModelState.AddModelError(string.Empty, message);

                if (model.DirectProductId.HasValue && model.DirectQuantity.HasValue)
                {
                    var prod = await _context.Products.FindAsync(model.DirectProductId.Value);
                    if (prod != null)
                    {
                        model.Items = new List<CartItemDto>
                        {
                            new CartItemDto
                            {
                                ProductId = prod.Id,
                                ProductName = prod.Name,
                                ProductSlug = prod.Slug,
                                ProductImageUrl = prod.MainImageUrl,
                                UnitPrice = prod.Price,
                                Quantity = model.DirectQuantity.Value
                            }
                        };
                    }
                }
                else
                {
                    var cart = await _cartService.GetCartAsync(userId, sessionId);
                    model.Items = cart.Items;
                }
                return View("Index", model);
            }

            return RedirectToAction(nameof(Success), new { code = orderCode });
        }

        // GET: /Checkout/Success?code=ORD-20261009-1234
        public async Task<IActionResult> Success(string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return RedirectToAction("Index", "Home");
            }

            var order = await _orderService.GetOrderByCodeAsync(code);
            if (order == null)
            {
                return NotFound();
            }

            var model = new OrderSuccessViewModel
            {
                OrderCode = order.OrderCode,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                PaymentStatus = order.PaymentStatus,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                ShippingAddress = $"{order.ShippingAddress}, {order.WardDistrict}, {order.ProvinceCity}",
                Items = order.Items.ToList()
            };

            return View(model);
        }

        // POST: /Checkout/ApplyCoupon
        [HttpPost]
        public async Task<IActionResult> ApplyCoupon([FromBody] string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
            {
                return Json(new { success = false, message = "Vui lòng nhập mã giảm giá" });
            }

            var code = couponCode.Trim().ToUpper();
            var coupon = await _context.Coupons.FirstOrDefaultAsync(c => c.Code.ToUpper() == code && c.IsActive);

            if (coupon == null)
            {
                return Json(new { success = false, message = "Mã giảm giá không tồn tại hoặc đã hết hạn" });
            }

            if (coupon.ExpiryDate.HasValue && coupon.ExpiryDate < DateTime.UtcNow)
            {
                return Json(new { success = false, message = "Mã giảm giá đã hết hạn sử dụng" });
            }

            return Json(new
            {
                success = true,
                message = "Áp dụng mã giảm giá thành công: " + coupon.Description,
                code = coupon.Code,
                type = coupon.Type.ToString(),
                value = coupon.DiscountValue,
                minSpend = coupon.MinimumSpend
            });
        }
    }
}
