using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebShop.Models.Entities
{
    public enum OrderStatus
    {
        Pending = 0,    // Chờ xác nhận
        Confirmed = 1,  // Đã xác nhận
        Preparing = 2,  // Đang chuẩn bị
        Shipping = 3,   // Đang giao hàng
        Delivered = 4,  // Đã giao
        Cancelled = 5   // Đã hủy
    }

    public enum PaymentStatus
    {
        Unpaid = 0, // Chưa thanh toán
        Paid = 1    // Đã thanh toán
    }

    public enum PaymentMethod
    {
        COD = 0,          // Thanh toán khi nhận hàng
        BankTransfer = 1  // Chuyển khoản ngân hàng
    }

    public class Order
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string OrderCode { get; set; } = string.Empty;

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Họ tên người nhận không được để trống")]
        [StringLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [StringLength(100)]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tỉnh/Thành phố không được để trống")]
        [StringLength(100)]
        public string ProvinceCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận/Huyện hoặc Phường/Xã không được để trống")]
        [StringLength(100)]
        public string WardDistrict { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống")]
        [StringLength(250)]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(500)]
        public string? OrderNotes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } = 30000;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        public string? TrackingNumber { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();

        [NotMapped]
        public string StatusDisplayName => Status switch
        {
            OrderStatus.Pending => "Chờ xác nhận",
            OrderStatus.Confirmed => "Đã xác nhận",
            OrderStatus.Preparing => "Đang chuẩn bị",
            OrderStatus.Shipping => "Đang giao hàng",
            OrderStatus.Delivered => "Đã giao thành công",
            OrderStatus.Cancelled => "Đã hủy",
            _ => "Không xác định"
        };

        [NotMapped]
        public string StatusBadgeClass => Status switch
        {
            OrderStatus.Pending => "bg-warning text-dark",
            OrderStatus.Confirmed => "bg-info text-dark",
            OrderStatus.Preparing => "bg-primary text-white",
            OrderStatus.Shipping => "bg-secondary text-white",
            OrderStatus.Delivered => "bg-success text-white",
            OrderStatus.Cancelled => "bg-danger text-white",
            _ => "bg-light text-dark"
        };

        [NotMapped]
        public string PaymentStatusDisplayName => PaymentStatus switch
        {
            PaymentStatus.Unpaid => "Chưa thanh toán",
            PaymentStatus.Paid => "Đã thanh toán",
            _ => "Chưa xác định"
        };
    }
}
