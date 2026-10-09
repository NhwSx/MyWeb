using System.ComponentModel.DataAnnotations;

namespace MyWebShop.Models.Entities
{
    public class ProductImage
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required]
        [StringLength(500)]
        public string ImageUrl { get; set; } = string.Empty;

        public int DisplayOrder { get; set; } = 0;
    }
}
