using Microsoft.EntityFrameworkCore;
using MyWebShop.Data;
using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;

namespace MyWebShop.Services
{
    public class CartService : ICartService
    {
        private readonly ApplicationDbContext _context;

        public CartService(ApplicationDbContext context)
        {
            _context = context;
        }

        private async Task<Cart> GetOrCreateCartEntityAsync(string? userId, string? sessionId)
        {
            Cart? cart = null;

            if (!string.IsNullOrEmpty(userId))
            {
                cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.UserId == userId);
            }
            else if (!string.IsNullOrEmpty(sessionId))
            {
                cart = await _context.Carts
                    .Include(c => c.Items)
                    .ThenInclude(i => i.Product)
                    .FirstOrDefaultAsync(c => c.SessionId == sessionId);
            }

            if (cart == null)
            {
                cart = new Cart
                {
                    UserId = userId,
                    SessionId = string.IsNullOrEmpty(userId) ? sessionId : null,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            return cart;
        }

        public async Task<CartViewModel> GetCartAsync(string? userId, string? sessionId)
        {
            var cart = await GetOrCreateCartEntityAsync(userId, sessionId);

            var items = cart.Items
                .Where(i => i.Product != null && i.Product.IsActive)
                .Select(i => new CartItemDto
                {
                    Id = i.Id,
                    ProductId = i.ProductId,
                    ProductName = i.Product!.Name,
                    ProductSlug = i.Product.Slug,
                    ProductImageUrl = i.Product.MainImageUrl,
                    UnitPrice = i.Product.Price, // Use current live price from product
                    OriginalPrice = i.Product.OriginalPrice,
                    Quantity = i.Quantity,
                    StockQuantity = i.Product.StockQuantity,
                    IsSelected = true
                })
                .ToList();

            var viewModel = new CartViewModel
            {
                Items = items
            };

            return viewModel;
        }

        public async Task<(bool Success, string Message, int CartCount)> AddToCartAsync(string? userId, string? sessionId, int productId, int quantity)
        {
            if (quantity <= 0)
            {
                return (false, "Số lượng sản phẩm không hợp lệ", 0);
            }

            var product = await _context.Products.FindAsync(productId);
            if (product == null || !product.IsActive)
            {
                return (false, "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh", 0);
            }

            if (product.StockQuantity <= 0)
            {
                return (false, "Sản phẩm hiện đang tạm hết hàng", 0);
            }

            var cart = await GetOrCreateCartEntityAsync(userId, sessionId);

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (cartItem != null)
            {
                if (cartItem.Quantity + quantity > product.StockQuantity)
                {
                    return (false, $"Kho chỉ còn {product.StockQuantity} sản phẩm (bạn đã có {cartItem.Quantity} trong giỏ)", cart.Items.Sum(i => i.Quantity));
                }
                cartItem.Quantity += quantity;
                cartItem.UnitPrice = product.Price;
            }
            else
            {
                if (quantity > product.StockQuantity)
                {
                    return (false, $"Kho chỉ còn {product.StockQuantity} sản phẩm", cart.Items.Sum(i => i.Quantity));
                }
                cart.Items.Add(new CartItem
                {
                    CartId = cart.Id,
                    ProductId = productId,
                    Quantity = quantity,
                    UnitPrice = product.Price
                });
            }

            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            int totalCount = cart.Items.Sum(i => i.Quantity);
            return (true, "Đã thêm sản phẩm vào giỏ hàng thành công!", totalCount);
        }

        public async Task<(bool Success, string Message, decimal LineTotal, decimal SubTotal, decimal GrandTotal, int CartCount)> UpdateQuantityAsync(string? userId, string? sessionId, int cartItemId, int quantity)
        {
            var cart = await GetOrCreateCartEntityAsync(userId, sessionId);
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item == null)
            {
                return (false, "Mục này không còn trong giỏ hàng", 0, 0, 0, 0);
            }

            var product = await _context.Products.FindAsync(item.ProductId);
            if (product == null)
            {
                return (false, "Sản phẩm không tồn tại", 0, 0, 0, 0);
            }

            if (quantity <= 0)
            {
                // Remove item
                _context.CartItems.Remove(item);
                await _context.SaveChangesAsync();
                var cartModelRem = await GetCartAsync(userId, sessionId);
                return (true, "Đã xóa sản phẩm khỏi giỏ hàng", 0, cartModelRem.SubTotal, cartModelRem.GrandTotal, cartModelRem.TotalCount);
            }

            if (quantity > product.StockQuantity)
            {
                return (false, $"Số lượng vượt quá tồn kho (tối đa {product.StockQuantity})", item.UnitPrice * item.Quantity, 0, 0, cart.Items.Sum(i => i.Quantity));
            }

            item.Quantity = quantity;
            item.UnitPrice = product.Price;
            cart.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            var cartModel = await GetCartAsync(userId, sessionId);
            decimal lineTotal = item.UnitPrice * item.Quantity;

            return (true, "Đã cập nhật số lượng", lineTotal, cartModel.SubTotal, cartModel.GrandTotal, cartModel.TotalCount);
        }

        public async Task<(bool Success, string Message, decimal SubTotal, decimal GrandTotal, int CartCount)> RemoveItemAsync(string? userId, string? sessionId, int cartItemId)
        {
            var cart = await GetOrCreateCartEntityAsync(userId, sessionId);
            var item = cart.Items.FirstOrDefault(i => i.Id == cartItemId);

            if (item != null)
            {
                _context.CartItems.Remove(item);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            var cartModel = await GetCartAsync(userId, sessionId);
            return (true, "Đã xóa sản phẩm khỏi giỏ hàng", cartModel.SubTotal, cartModel.GrandTotal, cartModel.TotalCount);
        }

        public async Task ClearCartAsync(string? userId, string? sessionId)
        {
            var cart = await GetOrCreateCartEntityAsync(userId, sessionId);
            if (cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                cart.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetCartCountAsync(string? userId, string? sessionId)
        {
            if (string.IsNullOrEmpty(userId) && string.IsNullOrEmpty(sessionId))
                return 0;

            IQueryable<Cart> query = _context.Carts.Include(c => c.Items);
            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(c => c.UserId == userId);
            }
            else
            {
                query = query.Where(c => c.SessionId == sessionId);
            }

            var cart = await query.FirstOrDefaultAsync();
            return cart?.Items.Sum(i => i.Quantity) ?? 0;
        }

        public async Task MigrateCartAsync(string sessionId, string userId)
        {
            if (string.IsNullOrEmpty(sessionId) || string.IsNullOrEmpty(userId))
                return;

            var guestCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.SessionId == sessionId);

            if (guestCart == null || !guestCart.Items.Any())
                return;

            var userCart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (userCart == null)
            {
                guestCart.UserId = userId;
                guestCart.SessionId = null;
                guestCart.UpdatedAt = DateTime.UtcNow;
            }
            else
            {
                foreach (var guestItem in guestCart.Items)
                {
                    var existingItem = userCart.Items.FirstOrDefault(i => i.ProductId == guestItem.ProductId);
                    if (existingItem != null)
                    {
                        existingItem.Quantity += guestItem.Quantity;
                    }
                    else
                    {
                        userCart.Items.Add(new CartItem
                        {
                            CartId = userCart.Id,
                            ProductId = guestItem.ProductId,
                            Quantity = guestItem.Quantity,
                            UnitPrice = guestItem.UnitPrice
                        });
                    }
                }
                userCart.UpdatedAt = DateTime.UtcNow;
                _context.Carts.Remove(guestCart);
            }

            await _context.SaveChangesAsync();
        }
    }
}
