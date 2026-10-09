using MyWebShop.Models.ViewModels;

namespace MyWebShop.Services
{
    public interface ICartService
    {
        Task<CartViewModel> GetCartAsync(string? userId, string? sessionId);
        Task<(bool Success, string Message, int CartCount)> AddToCartAsync(string? userId, string? sessionId, int productId, int quantity);
        Task<(bool Success, string Message, decimal LineTotal, decimal SubTotal, decimal GrandTotal, int CartCount)> UpdateQuantityAsync(string? userId, string? sessionId, int cartItemId, int quantity);
        Task<(bool Success, string Message, decimal SubTotal, decimal GrandTotal, int CartCount)> RemoveItemAsync(string? userId, string? sessionId, int cartItemId);
        Task ClearCartAsync(string? userId, string? sessionId);
        Task<int> GetCartCountAsync(string? userId, string? sessionId);
        Task MigrateCartAsync(string sessionId, string userId);
    }
}
