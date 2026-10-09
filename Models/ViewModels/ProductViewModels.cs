using MyWebShop.Models.Entities;

namespace MyWebShop.Models.ViewModels
{
    public class HomeViewModel
    {
        public List<Category> Categories { get; set; } = new();
        public List<Product> FeaturedProducts { get; set; } = new();
        public List<Product> BestSellerProducts { get; set; } = new();
        public List<Product> NewArrivalProducts { get; set; } = new();
        public List<Product> DiscountProducts { get; set; } = new();
    }

    public class ProductListViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();

        public int? SelectedCategoryId { get; set; }
        public string? CategorySlug { get; set; }
        public string? CurrentCategoryName { get; set; }

        public string? SearchQuery { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public bool InStockOnly { get; set; } = false;
        public string SortBy { get; set; } = "default"; // default, price_asc, price_desc, newest, popular

        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalItems { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
        public bool HasPreviousPage => PageNumber > 1;
        public bool HasNextPage => PageNumber < TotalPages;
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = null!;
        public List<Product> RelatedProducts { get; set; } = new();
        public List<Review> Reviews { get; set; } = new();
        public Dictionary<string, string> Specifications { get; set; } = new();
    }

    public class AddReviewViewModel
    {
        public int ProductId { get; set; }
        public int Rating { get; set; } = 5;
        public string Comment { get; set; } = string.Empty;
    }
}
