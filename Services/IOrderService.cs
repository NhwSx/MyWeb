using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;

namespace MyWebShop.Services
{
    public interface IOrderService
    {
        Task<(bool Success, string Message, string? OrderCode)> CreateOrderAsync(CheckoutViewModel model, string? userId, string? sessionId);
        Task<Order?> GetOrderByCodeAsync(string orderCode, string? userId = null);
        Task<Order?> GetOrderByIdAsync(int orderId);
        Task<List<Order>> GetUserOrdersAsync(string userId);
        Task<(bool Success, string Message)> CancelOrderAsync(int orderId, string userId);
        Task<(bool Success, string Message)> UpdateOrderStatusAsync(int orderId, OrderStatus status);
        Task<(bool Success, string Message)> UpdatePaymentStatusAsync(int orderId, PaymentStatus status);
    }
}
