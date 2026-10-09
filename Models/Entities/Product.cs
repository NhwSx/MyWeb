using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebShop.Models.Entities
{
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(220)]
        public string Slug { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mã sản phẩm (SKU) không được để trống")]
        [StringLength(50)]
        public string Sku { get; set; } = string.Empty;

        [StringLength(500)]
        public string? ShortDescription { get; set; }

        public string? Description { get; set; }

        [Required(ErrorMessage = "Giá bán không được để trống")]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000000, ErrorMessage = "Giá sản phẩm phải lớn hơn hoặc bằng 0")]
        public decimal Price { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Range(0, 1000000000, ErrorMessage = "Giá gốc phải lớn hơn hoặc bằng 0")]
        public decimal? OriginalPrice { get; set; }

        [Required(ErrorMessage = "Số lượng tồn kho không được để trống")]
        [Range(0, 1000000, ErrorMessage = "Số lượng tồn kho không hợp lệ")]
        public int StockQuantity { get; set; } = 0;

        [StringLength(500)]
        public string? MainImageUrl { get; set; }

        public string? SpecificationsJson { get; set; } // Key-value specs in JSON format

        public bool IsFeatured { get; set; } = false;
        public bool IsNew { get; set; } = false;
        public bool IsBestSeller { get; set; } = false;
        public bool IsActive { get; set; } = true;

        public double RatingAverage { get; set; } = 5.0;
        public int ReviewCount { get; set; } = 0;
        public int ViewCount { get; set; } = 0;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public int CategoryId { get; set; }
        public virtual Category? Category { get; set; }

        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
        public virtual ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        [NotMapped]
        public int DiscountPercentage => OriginalPrice.HasValue && OriginalPrice > Price && OriginalPrice > 0
            ? (int)Math.Round((1 - (Price / OriginalPrice.Value)) * 100)
            : 0;

        [NotMapped]
        public bool IsInStock => StockQuantity > 0;
    }
}
