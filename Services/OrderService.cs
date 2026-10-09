using Microsoft.EntityFrameworkCore;
using MyWebShop.Data;
using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;

namespace MyWebShop.Services
{
    public class OrderService : IOrderService
    {
        private readonly ApplicationDbContext _context;
        private readonly ICartService _cartService;

        public OrderService(ApplicationDbContext context, ICartService cartService)
        {
            _context = context;
            _cartService = cartService;
        }

        public async Task<(bool Success, string Message, string? OrderCode)> CreateOrderAsync(CheckoutViewModel model, string? userId, string? sessionId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orderItems = new List<OrderItem>();
                decimal subTotal = 0;

                if (model.DirectProductId.HasValue && model.DirectQuantity.HasValue && model.DirectQuantity.Value > 0)
                {
                    // Direct "Mua ngay"
                    var product = await _context.Products.FindAsync(model.DirectProductId.Value);
                    if (product == null || !product.IsActive)
                    {
                        return (false, "Sản phẩm không tồn tại hoặc đã ngừng kinh doanh", null);
                    }

                    if (product.StockQuantity < model.DirectQuantity.Value)
                    {
                        return (false, $"Sản phẩm '{product.Name}' chỉ còn {product.StockQuantity} trong kho", null);
                    }

                    // Decrement stock
                    product.StockQuantity -= model.DirectQuantity.Value;

                    var itemTotal = product.Price * model.DirectQuantity.Value;
                    subTotal += itemTotal;

                    orderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        ProductName = product.Name,
                        ProductImageUrl = product.MainImageUrl,
                        Quantity = model.DirectQuantity.Value,
                        UnitPrice = product.Price,
                        TotalPrice = itemTotal
                    });
                }
                else
                {
                    // Checkout from cart
                    var cart = await _cartService.GetCartAsync(userId, sessionId);
                    if (!cart.Items.Any())
                    {
                        return (false, "Giỏ hàng của bạn đang trống", null);
                    }

                    foreach (var cartItem in cart.Items)
                    {
                        var product = await _context.Products.FindAsync(cartItem.ProductId);
                        if (product == null || !product.IsActive)
                        {
                            return (false, $"Sản phẩm '{cartItem.ProductName}' hiện không khả dụng", null);
                        }

                        if (product.StockQuantity < cartItem.Quantity)
                        {
                            return (false, $"Sản phẩm '{product.Name}' chỉ còn {product.StockQuantity} trong kho", null);
                        }

                        // Decrement stock
                        product.StockQuantity -= cartItem.Quantity;

                        var itemTotal = product.Price * cartItem.Quantity;
                        subTotal += itemTotal;

                        orderItems.Add(new OrderItem
                        {
                            ProductId = product.Id,
                            ProductName = product.Name,
                            ProductImageUrl = product.MainImageUrl,
                            Quantity = cartItem.Quantity,
                            UnitPrice = product.Price,
                            TotalPrice = itemTotal
                        });
                    }
                }

                // Calculate shipping fee: Free ship for orders >= 10,000,000đ, otherwise 30,000đ
                decimal shippingFee = subTotal >= 10000000 ? 0 : 30000;

                // Validate and apply coupon if provided
                decimal discountAmount = 0;
                if (!string.IsNullOrWhiteSpace(model.CouponCode))
                {
                    var coupon = await _context.Coupons
                        .FirstOrDefaultAsync(c => c.Code.ToUpper() == model.CouponCode.Trim().ToUpper() && c.IsActive);

                    if (coupon != null && (coupon.ExpiryDate == null || coupon.ExpiryDate >= DateTime.UtcNow))
                    {
                        if (subTotal >= coupon.MinimumSpend)
                        {
                            if (coupon.Type == CouponType.Percentage)
                            {
                                discountAmount = (subTotal * coupon.DiscountValue) / 100m;
                            }
                            else
                            {
                                discountAmount = coupon.DiscountValue;
                            }
                        }
                    }
                }

                decimal grandTotal = Math.Max(0, subTotal + shippingFee - discountAmount);

                // Generate unique order code
                string datePrefix = DateTime.UtcNow.ToString("yyyyMMdd");
                string randomSuffix = new Random().Next(1000, 9999).ToString();
                string orderCode = $"ORD-{datePrefix}-{randomSuffix}";

                // Ensure unique
                while (await _context.Orders.AnyAsync(o => o.OrderCode == orderCode))
                {
                    randomSuffix = new Random().Next(1000, 9999).ToString();
                    orderCode = $"ORD-{datePrefix}-{randomSuffix}";
                }

                var order = new Order
                {
                    OrderCode = orderCode,
                    UserId = userId,
                    CustomerName = model.CustomerName.Trim(),
                    CustomerEmail = model.CustomerEmail.Trim(),
                    CustomerPhone = model.CustomerPhone.Trim(),
                    ProvinceCity = model.ProvinceCity.Trim(),
                    WardDistrict = model.WardDistrict.Trim(),
                    ShippingAddress = model.ShippingAddress.Trim(),
                    OrderNotes = model.OrderNotes?.Trim(),
                    SubTotal = subTotal,
                    ShippingFee = shippingFee,
                    DiscountAmount = discountAmount,
                    TotalAmount = grandTotal,
                    PaymentMethod = model.PaymentMethod,
                    PaymentStatus = PaymentStatus.Unpaid,
                    Status = OrderStatus.Pending,
                    CreatedAt = DateTime.UtcNow,
                    Items = orderItems
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync();

                // If this came from cart, clear cart
                if (!model.DirectProductId.HasValue)
                {
                    await _cartService.ClearCartAsync(userId, sessionId);
                }

                await transaction.CommitAsync();
                return (true, "Đặt hàng thành công!", orderCode);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, "Lỗi trong quá trình xử lý đơn hàng: " + ex.Message, null);
            }
        }

        public async Task<Order?> GetOrderByCodeAsync(string orderCode, string? userId = null)
        {
            var query = _context.Orders
                .Include(o => o.Items)
                .AsQueryable();

            if (!string.IsNullOrEmpty(userId))
            {
                query = query.Where(o => o.OrderCode == orderCode && o.UserId == userId);
            }
            else
            {
                query = query.Where(o => o.OrderCode == orderCode);
            }

            return await query.FirstOrDefaultAsync();
        }

        public async Task<Order?> GetOrderByIdAsync(int orderId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);
        }

        public async Task<List<Order>> GetUserOrdersAsync(string userId)
        {
            return await _context.Orders
                .Include(o => o.Items)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message)> CancelOrderAsync(int orderId, string userId)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == userId);

            if (order == null)
            {
                return (false, "Không tìm thấy đơn hàng cần hủy");
            }

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Confirmed)
            {
                return (false, "Đơn hàng đang chuẩn bị hoặc đang giao nên không thể hủy trực tuyến. Vui lòng liên hệ bộ phận hỗ trợ.");
            }

            // Restore product stock
            foreach (var item in order.Items)
            {
                if (item.ProductId.HasValue)
                {
                    var product = await _context.Products.FindAsync(item.ProductId.Value);
                    if (product != null)
                    {
                        product.StockQuantity += item.Quantity;
                    }
                }
            }

            order.Status = OrderStatus.Cancelled;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Đã hủy đơn hàng thành công");
        }

        public async Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, OrderStatus status)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return (false, "Không tìm thấy đơn hàng");
            }

            // If changing to Cancelled, return stock
            if (status == OrderStatus.Cancelled && order.Status != OrderStatus.Cancelled)
            {
                foreach (var item in order.Items)
                {
                    if (item.ProductId.HasValue)
                    {
                        var product = await _context.Products.FindAsync(item.ProductId.Value);
                        if (product != null)
                        {
                            product.StockQuantity += item.Quantity;
                        }
                    }
                }
            }

            // If changing to Delivered and COD, mark as Paid
            if (status == OrderStatus.Delivered && order.PaymentMethod == PaymentMethod.COD)
            {
                order.PaymentStatus = PaymentStatus.Paid;
            }

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Đã cập nhật trạng thái đơn hàng thành công");
        }

        public async Task<(bool Success, string Message)> UpdatePaymentStatusAsync(int orderId, PaymentStatus status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return (false, "Không tìm thấy đơn hàng");
            }

            order.PaymentStatus = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return (true, "Đã cập nhật trạng thái thanh toán thành công");
        }
    }
}
