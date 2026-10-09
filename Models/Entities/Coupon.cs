using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebShop.Models.Entities
{
    public enum CouponType
    {
        Percentage = 0,
        FixedAmount = 1
    }

    public class Coupon
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Code { get; set; } = string.Empty;

        [StringLength(200)]
        public string Description { get; set; } = string.Empty;

        public CouponType Type { get; set; } = CouponType.Percentage;

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountValue { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal MinimumSpend { get; set; } = 0;

        public DateTime? ExpiryDate { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
