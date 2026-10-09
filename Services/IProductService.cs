using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;

namespace MyWebShop.Services
{
    public interface IProductService
    {
        Task<HomeViewModel> GetHomeDataAsync();
        Task<ProductListViewModel> GetProductsAsync(string? categorySlug, string? search, decimal? minPrice, decimal? maxPrice, bool inStockOnly, string sortBy, int page, int pageSize);
        Task<ProductDetailViewModel?> GetProductDetailAsync(string slug);
        Task<Product?> GetProductByIdAsync(int id);
        Task<(bool Success, string Message)> AddReviewAsync(int productId, string userId, int rating, string comment);
        Task<List<Category>> GetAllCategoriesAsync();
    }
}
