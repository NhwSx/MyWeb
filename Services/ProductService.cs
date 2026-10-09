using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using MyWebShop.Data;
using MyWebShop.Models.Entities;
using MyWebShop.Models.ViewModels;

namespace MyWebShop.Services
{
    public class ProductService : IProductService
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HomeViewModel> GetHomeDataAsync()
        {
            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            var featuredProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsFeatured)
                .OrderByDescending(p => p.RatingAverage)
                .Take(8)
                .ToListAsync();

            var bestSellers = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsBestSeller)
                .OrderByDescending(p => p.ViewCount)
                .Take(8)
                .ToListAsync();

            var newArrivals = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.IsNew)
                .OrderByDescending(p => p.CreatedAt)
                .Take(8)
                .ToListAsync();

            var discountProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive && p.OriginalPrice > p.Price)
                .OrderByDescending(p => (p.OriginalPrice - p.Price))
                .Take(8)
                .ToListAsync();

            return new HomeViewModel
            {
                Categories = categories,
                FeaturedProducts = featuredProducts,
                BestSellerProducts = bestSellers,
                NewArrivalProducts = newArrivals,
                DiscountProducts = discountProducts
            };
        }

        public async Task<ProductListViewModel> GetProductsAsync(
            string? categorySlug,
            string? search,
            decimal? minPrice,
            decimal? maxPrice,
            bool inStockOnly,
            string sortBy,
            int page,
            int pageSize)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .Where(p => p.IsActive)
                .AsQueryable();

            string? currentCategoryName = null;
            int? selectedCategoryId = null;

            if (!string.IsNullOrEmpty(categorySlug))
            {
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.Slug == categorySlug);
                if (category != null)
                {
                    query = query.Where(p => p.CategoryId == category.Id);
                    currentCategoryName = category.Name;
                    selectedCategoryId = category.Id;
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                string term = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(term) ||
                                         p.Sku.ToLower().Contains(term) ||
                                         (p.ShortDescription != null && p.ShortDescription.ToLower().Contains(term)) ||
                                         (p.Category != null && p.Category.Name.ToLower().Contains(term)));
            }

            if (minPrice.HasValue && minPrice.Value > 0)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            if (inStockOnly)
            {
                query = query.Where(p => p.StockQuantity > 0);
            }

            // Sorting
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "popular" => query.OrderByDescending(p => p.ViewCount),
                "rating" => query.OrderByDescending(p => p.RatingAverage),
                _ => query.OrderByDescending(p => p.IsFeatured).ThenByDescending(p => p.Id)
            };

            int totalItems = await query.CountAsync();
            page = Math.Max(1, page);
            pageSize = pageSize > 0 ? pageSize : 12;

            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var categories = await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return new ProductListViewModel
            {
                Products = items,
                Categories = categories,
                SelectedCategoryId = selectedCategoryId,
                CategorySlug = categorySlug,
                CurrentCategoryName = currentCategoryName,
                SearchQuery = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                InStockOnly = inStockOnly,
                SortBy = sortBy,
                PageNumber = page,
                PageSize = pageSize,
                TotalItems = totalItems
            };
        }

        public async Task<ProductDetailViewModel?> GetProductDetailAsync(string slug)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images.OrderBy(i => i.DisplayOrder))
                .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive);

            if (product == null)
            {
                return null;
            }

            // Increment view count
            product.ViewCount++;
            await _context.SaveChangesAsync();

            var relatedProducts = await _context.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.IsActive)
                .Take(4)
                .ToListAsync();

            var reviews = await _context.Reviews
                .Include(r => r.User)
                .Where(r => r.ProductId == product.Id && r.IsApproved)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var specs = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(product.SpecificationsJson))
            {
                try
                {
                    specs = JsonSerializer.Deserialize<Dictionary<string, string>>(product.SpecificationsJson) ?? new();
                }
                catch
                {
                    // Fallback if not JSON format
                }
            }

            return new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = relatedProducts,
                Reviews = reviews,
                Specifications = specs
            };
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<(bool Success, string Message)> AddReviewAsync(int productId, string userId, int rating, string comment)
        {
            var product = await _context.Products.FindAsync(productId);
            if (product == null)
            {
                return (false, "Sản phẩm không tồn tại");
            }

            var review = new Review
            {
                ProductId = productId,
                UserId = userId,
                Rating = Math.Clamp(rating, 1, 5),
                Comment = comment.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsApproved = true
            };

            _context.Reviews.Add(review);
            await _context.SaveChangesAsync();

            // Recalculate product rating average
            var allReviews = await _context.Reviews.Where(r => r.ProductId == productId && r.IsApproved).ToListAsync();
            product.RatingAverage = Math.Round(allReviews.Average(r => r.Rating), 1);
            product.ReviewCount = allReviews.Count;
            await _context.SaveChangesAsync();

            return (true, "Cảm ơn bạn đã gửi đánh giá sản phẩm!");
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _context.Categories
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();
        }
    }
}
