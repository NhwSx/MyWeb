using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWebShop.Models.Entities;
using MyWebShop.Services;

namespace MyWebShop.Controllers
{
    [Authorize]
    public class OrderController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly UserManager<ApplicationUser> _userManager;

        public OrderController(IOrderService orderService, UserManager<ApplicationUser> userManager)
        {
            _orderService = orderService;
            _userManager = userManager;
        }

        // GET: /Order/History
        public async Task<IActionResult> History()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login", "Account");
            }

            var orders = await _orderService.GetUserOrdersAsync(user.Id);
            return View(orders);
        }

        // GET: /Order/Details/5 or /Order/Details?code=ORD-xxx
        public async Task<IActionResult> Details(int? id, string? code)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            Order? order = null;
            if (id.HasValue)
            {
                order = await _orderService.GetOrderByIdAsync(id.Value);
            }
            else if (!string.IsNullOrEmpty(code))
            {
                order = await _orderService.GetOrderByCodeAsync(code, user.Id);
            }

            if (order == null)
            {
                return NotFound();
            }

            // Security check: Customer can only view their own orders unless Admin
            if (order.UserId != user.Id && !User.IsInRole("Admin"))
            {
                return Forbid();
            }

            return View(order);
        }

        // POST: /Order/Cancel
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login", "Account");

            var (success, message) = await _orderService.CancelOrderAsync(id, user.Id);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
