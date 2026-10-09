using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;
using MyWebShop.Services;

namespace MyWebShop.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductService _productService;
        private readonly UserManager<ApplicationUser> _userManager;

        public ProductController(IProductService productService, UserManager<ApplicationUser> userManager)
        {
            _productService = productService;
            _userManager = userManager;
        }

        // GET: /Product or /Product?category=dien-thoai-tablet&search=iphone&minPrice=10000000&maxPrice=30000000&sortBy=price_asc&page=1
        public async Task<IActionResult> Index(
            string? category,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            bool inStockOnly = false,
            string sortBy = "default",
            int page = 1)
        {
            var model = await _productService.GetProductsAsync(category, search, minPrice, maxPrice, inStockOnly, sortBy, page, 12);
            return View(model);
        }

        // GET: /Product/Details/{slug}
        [Route("san-pham/{slug}")]
        [Route("Product/Details/{slug}")]
        public async Task<IActionResult> Details(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var model = await _productService.GetProductDetailAsync(slug);
            if (model == null)
            {
                return NotFound();
            }

            return View(model);
        }

        // POST: /Product/AddReview
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddReview(int productId, int rating, string comment, string slug)
        {
            if (string.IsNullOrWhiteSpace(comment))
            {
                TempData["ErrorMessage"] = "Vui lòng nhập nội dung đánh giá của bạn.";
                return RedirectToAction(nameof(Details), new { slug });
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return Challenge();
            }

            var (success, message) = await _productService.AddReviewAsync(productId, user.Id, rating, comment);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }

            return RedirectToAction(nameof(Details), new { slug });
        }
    }
}
