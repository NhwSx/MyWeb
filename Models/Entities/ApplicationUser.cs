using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MyWebShop.Models.Entities
{
    public class ApplicationUser : IdentityUser
    {
        [PersonalData]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [PersonalData]
        [StringLength(250)]
        public string? Address { get; set; }

        public string? AvatarUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<UserAddress> Addresses { get; set; } = new List<UserAddress>();
        public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();
    }
}
