using System.ComponentModel.DataAnnotations;

namespace MyWebShop.Models.Entities
{
    public class Review
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }

        [Required]
        [Range(1, 5, ErrorMessage = "Đánh giá từ 1 đến 5 sao")]
        public int Rating { get; set; } = 5;

        [Required(ErrorMessage = "Vui lòng nhập nhận xét của bạn")]
        [StringLength(1000)]
        public string Comment { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsApproved { get; set; } = true;
    }
}
