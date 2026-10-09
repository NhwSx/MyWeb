using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyWebShop.Data;
using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;
using MyWebShop.Services;

namespace MyWebShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly ICartService _cartService;
        private readonly ApplicationDbContext _context;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            ICartService cartService,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _cartService = cartService;
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

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var existingUser = await _userManager.FindByEmailAsync(model.Email);
            if (existingUser != null)
            {
                ModelState.AddModelError(nameof(model.Email), "Địa chỉ email này đã được sử dụng. Vui lòng đăng nhập hoặc dùng email khác.");
                return View(model);
            }

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName,
                PhoneNumber = model.PhoneNumber,
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (result.Succeeded)
            {
                // Assign Customer role
                await _userManager.AddToRoleAsync(user, "Customer");

                // Auto sign-in
                await _signInManager.SignInAsync(user, isPersistent: false);

                // Migrate guest cart if any
                var sessionId = Request.Cookies["CartSessionId"];
                if (!string.IsNullOrEmpty(sessionId))
                {
                    await _cartService.MigrateCartAsync(sessionId, user.Id);
                }

                TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Chào mừng bạn đến với cửa hàng.";
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View(new LoginViewModel { ReturnUrl = returnUrl });
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            returnUrl ??= model.ReturnUrl ?? Url.Content("~/");

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
                return View(model);
            }

            if (!user.IsActive)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản của bạn đã bị vô hiệu hoá. Vui lòng liên hệ quản trị viên.");
                return View(model);
            }

            var result = await _signInManager.PasswordSignInAsync(user.UserName!, model.Password, model.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded)
            {
                user.LastLoginAt = DateTime.UtcNow;
                await _userManager.UpdateAsync(user);

                // Migrate guest cart
                var sessionId = Request.Cookies["CartSessionId"];
                if (!string.IsNullOrEmpty(sessionId))
                {
                    await _cartService.MigrateCartAsync(sessionId, user.Id);
                }

                TempData["SuccessMessage"] = $"Chào mừng trở lại, {user.FullName}!";

                // If user is Admin, direct to Admin panel if no custom returnUrl
                if (await _userManager.IsInRoleAsync(user, "Admin") && (returnUrl == "/" || string.IsNullOrEmpty(returnUrl)))
                {
                    return RedirectToAction("Index", "Admin");
                }

                if (Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }

            if (result.IsLockedOut)
            {
                ModelState.AddModelError(string.Empty, "Tài khoản đã bị tạm khóa do nhập sai mật khẩu quá 5 lần. Vui lòng thử lại sau.");
                return View(model);
            }

            ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác.");
            return View(model);
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            TempData["SuccessMessage"] = "Đã đăng xuất tài khoản thành công.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/ForgotPassword
        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        // POST: /Account/ForgotPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                // Don't reveal user doesn't exist
                ViewBag.Message = "Nếu địa chỉ email tồn tại trong hệ thống, chúng tôi đã tạo liên kết khôi phục mật khẩu cho bạn.";
                return View("ForgotPasswordConfirmation");
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var callbackUrl = Url.Action("ResetPassword", "Account", new { email = model.Email, token = token }, Request.Scheme);

            ViewBag.ResetLink = callbackUrl;
            ViewBag.Token = token;
            ViewBag.Email = model.Email;
            ViewBag.Message = "Đã tạo liên kết đặt lại mật khẩu thành công!";

            return View("ForgotPasswordConfirmation");
        }

        // GET: /Account/ResetPassword
        [HttpGet]
        public IActionResult ResetPassword(string? token = null, string? email = null)
        {
            if (token == null || email == null)
            {
                TempData["ErrorMessage"] = "Yêu cầu khôi phục mật khẩu không hợp lệ.";
                return RedirectToAction("Login");
            }

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        // POST: /Account/ResetPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["SuccessMessage"] = "Mật khẩu đã được thiết lập lại thành công. Vui lòng đăng nhập!";
                return RedirectToAction("Login");
            }

            var result = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công! Bạn có thể đăng nhập bằng mật khẩu mới.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return View(model);
        }

        // GET: /Account/Profile
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var model = new ProfileViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                Address = user.Address
            };

            var userOrders = await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == user.Id)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var userAddresses = await _context.UserAddresses
                .Where(a => a.UserId == user.Id)
                .ToListAsync();

            ViewBag.Orders = userOrders;
            ViewBag.Addresses = userAddresses;

            return View(model);
        }

        // POST: /Account/Profile
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile(ProfileViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            if (!ModelState.IsValid)
            {
                ViewBag.Orders = await _context.Orders.Where(o => o.UserId == user.Id).OrderByDescending(o => o.CreatedAt).ToListAsync();
                ViewBag.Addresses = await _context.UserAddresses.Where(a => a.UserId == user.Id).ToListAsync();
                return View(model);
            }

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;
            user.Address = model.Address;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = "Cập nhật thông tin cá nhân thành công!";
                return RedirectToAction(nameof(Profile));
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            ViewBag.Orders = await _context.Orders.Where(o => o.UserId == user.Id).OrderByDescending(o => o.CreatedAt).ToListAsync();
            ViewBag.Addresses = await _context.UserAddresses.Where(a => a.UserId == user.Id).ToListAsync();
            return View(model);
        }

        // POST: /Account/ChangePassword
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Thông tin đổi mật khẩu không hợp lệ.";
                return RedirectToAction(nameof(Profile));
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return RedirectToAction("Login");
            }

            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["SuccessMessage"] = "Đổi mật khẩu thành công!";
                return RedirectToAction(nameof(Profile));
            }

            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            TempData["ErrorMessage"] = "Đổi mật khẩu thất bại: " + errors;
            return RedirectToAction(nameof(Profile));
        }

        // POST: /Account/AddAddress
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAddress(UserAddress address)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return RedirectToAction("Login");

            address.UserId = user.Id;
            if (address.IsDefault)
            {
                var existing = await _context.UserAddresses.Where(a => a.UserId == user.Id).ToListAsync();
                foreach (var a in existing) a.IsDefault = false;
            }

            _context.UserAddresses.Add(address);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Đã thêm địa chỉ mới thành công!";
            return RedirectToAction(nameof(Profile));
        }
    }
}
