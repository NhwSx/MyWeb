using System.ComponentModel.DataAnnotations;
using MyWebShop.Models.Entities;

namespace MyWebShop.Models.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận")]
        [Display(Name = "Họ và tên")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email nhận thông tin đơn hàng")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại liên hệ giao hàng")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Tỉnh / Thành phố")]
        [Display(Name = "Tỉnh / Thành phố")]
        public string ProvinceCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập Quận / Huyện / Phường / Xã")]
        [Display(Name = "Quận / Huyện")]
        public string WardDistrict { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ cụ thể (số nhà, tên đường)")]
        [Display(Name = "Địa chỉ chi tiết")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Display(Name = "Ghi chú đơn hàng")]
        public string? OrderNotes { get; set; }

        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.COD;

        public string? CouponCode { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal SubTotal { get; set; }
        public decimal ShippingFee { get; set; } = 30000;
        public decimal TotalAmount { get; set; }

        public List<CartItemDto> Items { get; set; } = new();

        // For Buy Now direct checkout
        public int? DirectProductId { get; set; }
        public int? DirectQuantity { get; set; }
    }

    public class OrderSuccessViewModel
    {
        public string OrderCode { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public PaymentMethod PaymentMethod { get; set; }
        public PaymentStatus PaymentStatus { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new();
    }
}
