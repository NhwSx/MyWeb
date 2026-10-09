using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyWebShop.Models.Entities;

namespace MyWebShop.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Category> Categories { get; set; } = null!;
        public DbSet<Product> Products { get; set; } = null!;
        public DbSet<ProductImage> ProductImages { get; set; } = null!;
        public DbSet<Cart> Carts { get; set; } = null!;
        public DbSet<CartItem> CartItems { get; set; } = null!;
        public DbSet<Order> Orders { get; set; } = null!;
        public DbSet<OrderItem> OrderItems { get; set; } = null!;
        public DbSet<UserAddress> UserAddresses { get; set; } = null!;
        public DbSet<Review> Reviews { get; set; } = null!;
        public DbSet<Coupon> Coupons { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Indexes
            builder.Entity<Product>()
                .HasIndex(p => p.Slug)
                .IsUnique();

            builder.Entity<Product>()
                .HasIndex(p => p.Sku)
                .IsUnique();

            builder.Entity<Category>()
                .HasIndex(c => c.Slug)
                .IsUnique();

            builder.Entity<Order>()
                .HasIndex(o => o.OrderCode)
                .IsUnique();

            builder.Entity<Coupon>()
                .HasIndex(c => c.Code)
                .IsUnique();

            // Category -> Products
            builder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // Product -> Images
            builder.Entity<ProductImage>()
                .HasOne(pi => pi.Product)
                .WithMany(p => p.Images)
                .HasForeignKey(pi => pi.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // Cart -> Items
            builder.Entity<CartItem>()
                .HasOne(ci => ci.Cart)
                .WithMany(c => c.Items)
                .HasForeignKey(ci => ci.CartId)
                .OnDelete(DeleteBehavior.Cascade);

            // Order -> OrderItems
            builder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            // Product -> Reviews
            builder.Entity<Review>()
                .HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Cascade);

            // User -> Addresses
            builder.Entity<UserAddress>()
                .HasOne(ua => ua.User)
                .WithMany(u => u.Addresses)
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
