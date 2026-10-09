namespace MyWebShop.Models.ViewModels
{
    public class CartItemDto
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductSlug { get; set; } = string.Empty;
        public string? ProductImageUrl { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal? OriginalPrice { get; set; }
        public int Quantity { get; set; }
        public int StockQuantity { get; set; }
        public decimal TotalPrice => UnitPrice * Quantity;
        public bool IsSelected { get; set; } = true;
    }

    public class CartViewModel
    {
        public List<CartItemDto> Items { get; set; } = new();
        public decimal SubTotal => Items.Sum(i => i.TotalPrice);
        public decimal ShippingFee => SubTotal >= 10000000 || SubTotal == 0 ? 0 : 30000;
        public decimal DiscountAmount { get; set; } = 0;
        public string? AppliedCouponCode { get; set; }
        public decimal GrandTotal => Math.Max(0, SubTotal + ShippingFee - DiscountAmount);
        public int TotalCount => Items.Sum(i => i.Quantity);
    }

    public class AddToCartRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }

    public class UpdateCartItemRequest
    {
        public int CartItemId { get; set; }
        public int Quantity { get; set; }
    }
}
