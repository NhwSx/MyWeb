using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyWebShop.Models.Entities
{
    public class CartItem
    {
        public int Id { get; set; }

        public int CartId { get; set; }
        public virtual Cart? Cart { get; set; }

        public int ProductId { get; set; }
        public virtual Product? Product { get; set; }

        [Required]
        [Range(1, 1000, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }

        [NotMapped]
        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
