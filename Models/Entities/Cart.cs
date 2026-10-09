using System.ComponentModel.DataAnnotations;

namespace MyWebShop.Models.Entities
{
    public class Cart
    {
        public int Id { get; set; }

        public string? UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        [StringLength(100)]
        public string? SessionId { get; set; } // for guest users

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public virtual ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}
