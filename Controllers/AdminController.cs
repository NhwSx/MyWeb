using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebShop.Data;
using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;
using MyWebShop.Services;
using System.Text.RegularExpressions;

namespace MyWebShop.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IOrderService _orderService;

        public AdminController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IOrderService orderService)
        {
            _context = context;
            _userManager = userManager;
            _orderService = orderService;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var totalOrders = await _context.Orders.CountAsync();
            var pendingOrders = await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending);
            var totalCustomers = await _userManager.GetUsersInRoleAsync("Customer");
            var totalProducts = await _context.Products.CountAsync(p => p.IsActive);
            var lowStockCount = await _context.Products.CountAsync(p => p.IsActive && p.StockQuantity <= 20);

            // Total revenue from Delivered / Paid orders
            var totalRevenue = await _context.Orders
                .Where(o => o.Status != OrderStatus.Cancelled)
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

            var recentOrders = await _context.Orders
                .Include(o => o.Items)
                .OrderByDescending(o => o.CreatedAt)
                .Take(7)
                .ToListAsync();

            var lowStockProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.StockQuantity <= 20)
                .OrderBy(p => p.StockQuantity)
                .Take(5)
                .ToListAsync();

            // Prepare 6-month revenue chart data
            var labels = new List<string>();
            var data = new List<decimal>();
            var now = DateTime.UtcNow;

            for (int i = 5; i >= 0; i--)
            {
                var monthDate = now.AddMonths(-i);
                var monthLabel = $"T{monthDate.Month}/{monthDate.Year}";
                labels.Add(monthLabel);

                var monthRev = await _context.Orders
                    .Where(o => o.CreatedAt.Month == monthDate.Month &&
                                o.CreatedAt.Year == monthDate.Year &&
                                o.Status != OrderStatus.Cancelled)
                    .SumAsync(o => (decimal?)o.TotalAmount) ?? 0;

                data.Add(monthRev);
            }

            // Order status distribution
            var statusLabels = new List<string> { "Chờ xác nhận", "Đã xác nhận", "Đang chuẩn bị", "Đang giao", "Đã giao", "Đã hủy" };
            var statusCounts = new List<int>
            {
                await _context.Orders.CountAsync(o => o.Status == OrderStatus.Pending),
                await _context.Orders.CountAsync(o => o.Status == OrderStatus.Confirmed),
                await _context.Orders.CountAsync(o => o.Status == OrderStatus.Preparing),
                await _context.Orders.CountAsync(o => o.Status == OrderStatus.Shipping),
                await _context.Orders.CountAsync(o => o.Status == OrderStatus.Delivered),
                await _context.Orders.CountAsync(o => o.Status == OrderStatus.Cancelled)
            };

            var viewModel = new AdminDashboardViewModel
            {
                TotalRevenue = totalRevenue,
                TotalOrders = totalOrders,
                PendingOrders = pendingOrders,
                TotalCustomers = totalCustomers.Count,
                TotalProducts = totalProducts,
                LowStockProductsCount = lowStockCount,
                RecentOrders = recentOrders,
                LowStockProducts = lowStockProducts,
                RevenueChartLabels = labels,
                RevenueChartData = data,
                OrderStatusLabels = statusLabels,
                OrderStatusCounts = statusCounts
            };

            return View(viewModel);
        }

        // GET: /Admin/Products
        public async Task<IActionResult> Products(string? search, int? categoryId)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(p => p.Name.Contains(search) || p.Sku.Contains(search));
            }

            if (categoryId.HasValue)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            var products = await query.OrderByDescending(p => p.Id).ToListAsync();
            ViewBag.Categories = await _context.Categories.ToListAsync();
            ViewBag.Search = search;
            ViewBag.CategoryId = categoryId;

            return View(products);
        }

        // GET: /Admin/CreateProduct
        [HttpGet]
        public async Task<IActionResult> CreateProduct()
        {
            var model = new ProductFormViewModel
            {
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync(),
                StockQuantity = 10,
                IsActive = true
            };
            return View(model);
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(ProductFormViewModel model)
        {
            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Tên sản phẩm không được để trống");
            }

            if (string.IsNullOrWhiteSpace(model.Sku))
            {
                ModelState.AddModelError(nameof(model.Sku), "Mã SKU không được để trống");
            }
            else if (await _context.Products.AnyAsync(p => p.Sku == model.Sku.Trim()))
            {
                ModelState.AddModelError(nameof(model.Sku), "Mã SKU này đã tồn tại");
            }

            if (model.Price < 0)
            {
                ModelState.AddModelError(nameof(model.Price), "Giá sản phẩm phải lớn hơn hoặc bằng 0");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            string slug = GenerateSlug(model.Name);
            int count = 1;
            while (await _context.Products.AnyAsync(p => p.Slug == slug))
            {
                slug = $"{GenerateSlug(model.Name)}-{count++}";
            }

            var product = new Product
            {
                Name = model.Name.Trim(),
                Slug = slug,
                Sku = model.Sku.Trim().ToUpper(),
                CategoryId = model.CategoryId,
                Price = model.Price,
                OriginalPrice = model.OriginalPrice,
                StockQuantity = model.StockQuantity,
                ShortDescription = model.ShortDescription,
                Description = model.Description,
                MainImageUrl = string.IsNullOrWhiteSpace(model.MainImageUrl)
                    ? "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=800&auto=format&fit=crop&q=80"
                    : model.MainImageUrl.Trim(),
                SpecificationsJson = model.SpecificationsJson,
                IsFeatured = model.IsFeatured,
                IsNew = model.IsNew,
                IsBestSeller = model.IsBestSeller,
                IsActive = model.IsActive,
                CreatedAt = DateTime.UtcNow
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã thêm sản phẩm mới thành công!";
            return RedirectToAction(nameof(Products));
        }

        // GET: /Admin/EditProduct/5
        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null) return NotFound();

            var model = new ProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Sku = product.Sku,
                CategoryId = product.CategoryId,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                StockQuantity = product.StockQuantity,
                ShortDescription = product.ShortDescription,
                Description = product.Description,
                MainImageUrl = product.MainImageUrl,
                SpecificationsJson = product.SpecificationsJson,
                IsFeatured = product.IsFeatured,
                IsNew = product.IsNew,
                IsBestSeller = product.IsBestSeller,
                IsActive = product.IsActive,
                Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync()
            };

            return View(model);
        }

        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(ProductFormViewModel model)
        {
            var product = await _context.Products.FindAsync(model.Id);
            if (product == null) return NotFound();

            if (string.IsNullOrWhiteSpace(model.Name))
            {
                ModelState.AddModelError(nameof(model.Name), "Tên sản phẩm không được để trống");
            }

            if (await _context.Products.AnyAsync(p => p.Sku == model.Sku.Trim() && p.Id != model.Id))
            {
                ModelState.AddModelError(nameof(model.Sku), "Mã SKU này đã tồn tại ở sản phẩm khác");
            }

            if (!ModelState.IsValid)
            {
                model.Categories = await _context.Categories.Where(c => c.IsActive).ToListAsync();
                return View(model);
            }

            product.Name = model.Name.Trim();
            product.Sku = model.Sku.Trim().ToUpper();
            product.CategoryId = model.CategoryId;
            product.Price = model.Price;
            product.OriginalPrice = model.OriginalPrice;
            product.StockQuantity = model.StockQuantity;
            product.ShortDescription = model.ShortDescription;
            product.Description = model.Description;
            if (!string.IsNullOrWhiteSpace(model.MainImageUrl))
            {
                product.MainImageUrl = model.MainImageUrl.Trim();
            }
            product.SpecificationsJson = model.SpecificationsJson;
            product.IsFeatured = model.IsFeatured;
            product.IsNew = model.IsNew;
            product.IsBestSeller = model.IsBestSeller;
            product.IsActive = model.IsActive;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
            return RedirectToAction(nameof(Products));
        }

        // POST: /Admin/DeleteProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                // Soft delete by setting IsActive = false to maintain order history integrity
                product.IsActive = false;
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã ẩn sản phẩm '{product.Name}' khỏi cửa hàng.";
            }
            return RedirectToAction(nameof(Products));
        }

        // GET: /Admin/Categories
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
            return View(categories);
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (string.IsNullOrWhiteSpace(category.Name))
            {
                TempData["ErrorMessage"] = "Tên danh mục không được để trống";
                return RedirectToAction(nameof(Categories));
            }

            category.Slug = GenerateSlug(category.Name);
            _context.Categories.Add(category);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã tạo danh mục mới thành công!";
            return RedirectToAction(nameof(Categories));
        }

        // GET: /Admin/Orders
        public async Task<IActionResult> Orders(OrderStatus? status, string? search)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .AsQueryable();

            if (status.HasValue)
            {
                query = query.Where(o => o.Status == status.Value);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(o => o.OrderCode.Contains(search) ||
                                         o.CustomerName.Contains(search) ||
                                         o.CustomerPhone.Contains(search));
            }

            var orders = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
            ViewBag.CurrentStatus = status;
            ViewBag.Search = search;

            return View(orders);
        }

        // GET: /Admin/OrderDetail/5
        public async Task<IActionResult> OrderDetail(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null) return NotFound();
            return View(order);
        }

        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, OrderStatus status)
        {
            var (success, message) = await _orderService.UpdateOrderStatusAsync(id, status);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        // POST: /Admin/UpdatePaymentStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdatePaymentStatus(int id, PaymentStatus paymentStatus)
        {
            var (success, message) = await _orderService.UpdatePaymentStatusAsync(id, paymentStatus);
            if (success)
            {
                TempData["SuccessMessage"] = message;
            }
            else
            {
                TempData["ErrorMessage"] = message;
            }
            return RedirectToAction(nameof(OrderDetail), new { id });
        }

        // GET: /Admin/Users
        public async Task<IActionResult> Users(string? search)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                usersQuery = usersQuery.Where(u => u.Email!.Contains(search) || u.FullName.Contains(search));
            }

            var users = await usersQuery.OrderByDescending(u => u.CreatedAt).ToListAsync();
            var list = new List<UserListViewModel>();

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                list.Add(new UserListViewModel
                {
                    Id = u.Id,
                    Email = u.Email ?? "",
                    FullName = u.FullName,
                    PhoneNumber = u.PhoneNumber,
                    IsLockedOut = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                    Roles = roles.ToList(),
                    CreatedAt = u.CreatedAt
                });
            }

            ViewBag.Search = search;
            return View(list);
        }

        // POST: /Admin/ToggleLockUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleLockUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            if (user.LockoutEnd.HasValue && user.LockoutEnd > DateTimeOffset.UtcNow)
            {
                // Unlock
                await _userManager.SetLockoutEndDateAsync(user, null);
                TempData["SuccessMessage"] = $"Đã mở khóa tài khoản {user.Email}";
            }
            else
            {
                // Lock
                await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
                TempData["SuccessMessage"] = $"Đã khóa tài khoản {user.Email}";
            }

            return RedirectToAction(nameof(Users));
        }

        private static string GenerateSlug(string text)
        {
            string str = text.ToLowerInvariant();
            // Replace Vietnamese characters
            string[] vietnamese = new[] { "á|à|ả|ã|ạ|ă|ắ|ằ|ẳ|ẵ|ặ|â|ấ|ầ|ẩ|ẫ|ậ", "đ", "é|è|ẻ|ẽ|ẹ|ê|ế|ề|ể|ễ|ệ", "í|ì|ỉ|ĩ|ị", "ó|ò|ỏ|õ|ọ|ô|ố|ồ|ổ|ỗ|ộ|ơ|ớ|ờ|ở|ỡ|ợ", "ú|ù|ủ|ũ|ụ|ư|ứ|ừ|ử|ữ|ự", "ý|ỳ|ỷ|ỹ|ỵ" };
            string[] replacements = new[] { "a", "d", "e", "i", "o", "u", "y" };
            for (int i = 0; i < vietnamese.Length; i++)
            {
                str = Regex.Replace(str, vietnamese[i], replacements[i]);
            }
            str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
            str = Regex.Replace(str, @"\s+", "-").Trim('-');
            return str;
        }
    }
}
