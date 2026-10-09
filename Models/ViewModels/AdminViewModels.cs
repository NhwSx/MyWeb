using MyWebShop.Models.Entities;

namespace MyWebShop.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public decimal TotalRevenue { get; set; }
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int LowStockProductsCount { get; set; }

        public List<Order> RecentOrders { get; set; } = new();
        public List<Product> LowStockProducts { get; set; } = new();

        // Chart Data
        public List<string> RevenueChartLabels { get; set; } = new();
        public List<decimal> RevenueChartData { get; set; } = new();
        public List<string> OrderStatusLabels { get; set; } = new();
        public List<int> OrderStatusCounts { get; set; } = new();
    }

    public class ProductFormViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int CategoryId { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int StockQuantity { get; set; }
        public string? ShortDescription { get; set; }
        public string? Description { get; set; }
        public string? MainImageUrl { get; set; }
        public string? SpecificationsJson { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsNew { get; set; }
        public bool IsBestSeller { get; set; }
        public bool IsActive { get; set; } = true;
        public List<Category> Categories { get; set; } = new();
    }

    public class OrderFilterViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public OrderStatus? StatusFilter { get; set; }
        public string? SearchKeyword { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }

    public class UserListViewModel
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public bool IsLockedOut { get; set; }
        public List<string> Roles { get; set; } = new();
        public DateTime CreatedAt { get; set; }
    }
}
