using System.ComponentModel.DataAnnotations;

namespace MyWebShop.Models.Entities
{
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string Slug { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(250)]
        public string? IconClass { get; set; } // Bootstrap icon class, e.g. bi-phone

        [StringLength(500)]
        public string? ImageUrl { get; set; }

        public int DisplayOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
