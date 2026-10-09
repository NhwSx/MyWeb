using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyWebShop.Models.Entities;

namespace MyWebShop.Data
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Ensure database is created with tables
            await context.Database.EnsureCreatedAsync();

            // 1. Seed Roles
            string[] roles = { "Admin", "Customer" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // 2. Seed Admin User
            var adminEmail = "admin@eshop.com";
            var adminUser = await userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true,
                    FullName = "Quản Trị Viên Cửa Hàng",
                    PhoneNumber = "0901234567",
                    Address = "72 Lê Thánh Tôn, Phường Bến Nghé, Quận 1, TP. Hồ Chí Minh",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(adminUser, "Admin@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(adminUser, "Admin");
                }
            }

            // 3. Seed Normal Customer User
            var customerEmail = "user@eshop.com";
            var customerUser = await userManager.FindByEmailAsync(customerEmail);
            if (customerUser == null)
            {
                customerUser = new ApplicationUser
                {
                    UserName = customerEmail,
                    Email = customerEmail,
                    EmailConfirmed = true,
                    FullName = "Nguyễn Văn An",
                    PhoneNumber = "0987654321",
                    Address = "123 Cầu Giấy, Phường Dịch Vọng, Quận Cầu Giấy, Hà Nội",
                    CreatedAt = DateTime.UtcNow
                };
                var result = await userManager.CreateAsync(customerUser, "User@123456");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(customerUser, "Customer");

                    // Seed address for user
                    context.UserAddresses.Add(new UserAddress
                    {
                        UserId = customerUser.Id,
                        FullName = "Nguyễn Văn An",
                        PhoneNumber = "0987654321",
                        ProvinceCity = "Hà Nội",
                        WardDistrict = "Quận Cầu Giấy, Phường Dịch Vọng",
                        StreetAddress = "123 Cầu Giấy",
                        IsDefault = true
                    });
                    await context.SaveChangesAsync();
                }
            }

            // 4. Seed Categories
            if (!await context.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new Category
                    {
                        Name = "Điện thoại & Tablet",
                        Slug = "dien-thoai-tablet",
                        Description = "Điện thoại thông minh, iPad, máy tính bảng chính hãng cao cấp",
                        IconClass = "bi-phone",
                        ImageUrl = "https://images.unsplash.com/photo-1511707171634-5f897ff02aa9?w=600&auto=format&fit=crop&q=80",
                        DisplayOrder = 1
                    },
                    new Category
                    {
                        Name = "Laptop & Máy tính",
                        Slug = "laptop-may-tinh",
                        Description = "Laptop gaming, văn phòng, đồ hoạ mỏng nhẹ hiệu năng mạnh",
                        IconClass = "bi-laptop",
                        ImageUrl = "https://images.unsplash.com/photo-1496181133206-80ce9b88a853?w=600&auto=format&fit=crop&q=80",
                        DisplayOrder = 2
                    },
                    new Category
                    {
                        Name = "Tai nghe & Âm thanh",
                        Slug = "tai-nghe-am-thanh",
                        Description = "Tai nghe True Wireless, chống ồn chủ động, loa Bluetooth đỉnh cao",
                        IconClass = "bi-headphones",
                        ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&auto=format&fit=crop&q=80",
                        DisplayOrder = 3
                    },
                    new Category
                    {
                        Name = "Đồng hồ thông minh",
                        Slug = "dong-ho-thong-minh",
                        Description = "Smartwatch theo dõi sức khoẻ, thời trang và thể thao",
                        IconClass = "bi-smartwatch",
                        ImageUrl = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600&auto=format&fit=crop&q=80",
                        DisplayOrder = 4
                    },
                    new Category
                    {
                        Name = "Phụ kiện công nghệ",
                        Slug = "phu-kien-cong-nghe",
                        Description = "Sạc nhanh, bàn phím cơ, chuột gaming, hub chuyển đổi",
                        IconClass = "bi-mouse",
                        ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=600&auto=format&fit=crop&q=80",
                        DisplayOrder = 5
                    },
                    new Category
                    {
                        Name = "Thiết bị Smart Home",
                        Slug = "thiet-bi-smart-home",
                        Description = "Camera an ninh, robot hút bụi, đèn thông minh tiện nghi",
                        IconClass = "bi-house-gear",
                        ImageUrl = "https://images.unsplash.com/photo-1558002038-1055907df827?w=600&auto=format&fit=crop&q=80",
                        DisplayOrder = 6
                    }
                };

                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            // 5. Seed Products
            if (!await context.Products.AnyAsync())
            {
                var catPhone = await context.Categories.FirstAsync(c => c.Slug == "dien-thoai-tablet");
                var catLaptop = await context.Categories.FirstAsync(c => c.Slug == "laptop-may-tinh");
                var catAudio = await context.Categories.FirstAsync(c => c.Slug == "tai-nghe-am-thanh");
                var catWatch = await context.Categories.FirstAsync(c => c.Slug == "dong-ho-thong-minh");
                var catAcc = await context.Categories.FirstAsync(c => c.Slug == "phu-kien-cong-nghe");
                var catHome = await context.Categories.FirstAsync(c => c.Slug == "thiet-bi-smart-home");

                var products = new List<Product>
                {
                    // Phones
                    new Product
                    {
                        Name = "iPhone 15 Pro Max 256GB Titan Tự Nhiên",
                        Slug = "iphone-15-pro-max-256gb-titan-tu-nhien",
                        Sku = "IP15PM-256-NAT",
                        CategoryId = catPhone.Id,
                        Price = 29990000,
                        OriginalPrice = 34990000,
                        StockQuantity = 45,
                        ShortDescription = "Khung titan siêu bền nhẹ, Chip A17 Pro mạnh mẽ nhất, camera zoom quang học 5x chuyên nghiệp.",
                        Description = "iPhone 15 Pro Max là siêu phẩm đỉnh cao với thiết kế vỏ Titan chuẩn hàng không vũ trụ, cổng USB-C tốc độ 10Gbps và nút Action đa nhiệm thông minh. Màn hình Super Retina XDR 6.7 inch 120Hz ProMotion mang lại trải nghiệm mượt mà không đối thủ.",
                        MainImageUrl = "https://images.unsplash.com/photo-1695048133142-1a20484d2569?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Màn hình\":\"6.7 inch OLED Super Retina XDR 120Hz\",\"Chip CPU\":\"Apple A17 Pro 3nm\",\"Bộ nhớ trong\":\"256 GB\",\"Camera sau\":\"Chính 48MP + 12MP + 12MP zoom 5x\",\"Pin\":\"4.422 mAh, sạc nhanh 20W\"}",
                        IsFeatured = true,
                        IsBestSeller = true,
                        IsNew = true,
                        RatingAverage = 4.9,
                        ReviewCount = 88,
                        ViewCount = 1250
                    },
                    new Product
                    {
                        Name = "Samsung Galaxy S24 Ultra 5G 12GB/256GB Titanium Xám",
                        Slug = "samsung-galaxy-s24-ultra-5g-titanium-xam",
                        Sku = "SS-S24U-256-GRY",
                        CategoryId = catPhone.Id,
                        Price = 27490000,
                        OriginalPrice = 31990000,
                        StockQuantity = 32,
                        ShortDescription = "Quyền năng Galaxy AI dẫn đầu xu hướng, bút S Pen tích hợp, camera 200MP zoom mắt thần bóng đêm.",
                        Description = "Samsung Galaxy S24 Ultra mở ra kỷ nguyên trí tuệ nhân tạo Galaxy AI với tính năng Khoanh vùng tìm kiếm (Circle to Search), phiên dịch trực tiếp cuộc gọi và trợ lý chỉnh ảnh chuyên nghiệp. Khung viền Titan phẳng sang trọng cùng kính cường lực Corning Gorilla Armor chống phản chiếu vượt trội.",
                        MainImageUrl = "https://images.unsplash.com/photo-1610945265064-0e34e5519bbf?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Màn hình\":\"6.8 inch Dynamic AMOLED 2X 120Hz\",\"Chip CPU\":\"Snapdragon 8 Gen 3 for Galaxy\",\"Bộ nhớ RAM\":\"12 GB\",\"Bộ nhớ trong\":\"256 GB\",\"Camera sau\":\"200MP + 50MP + 12MP + 10MP\",\"Pin\":\"5.000 mAh, sạc nhanh 45W\"}",
                        IsFeatured = true,
                        IsBestSeller = true,
                        IsNew = false,
                        RatingAverage = 4.8,
                        ReviewCount = 64,
                        ViewCount = 980
                    },
                    new Product
                    {
                        Name = "iPad Pro 11 inch M4 256GB Wi-Fi Space Black",
                        Slug = "ipad-pro-11-inch-m4-256gb-space-black",
                        Sku = "IPAD-M4-11-256",
                        CategoryId = catPhone.Id,
                        Price = 26990000,
                        OriginalPrice = 28990000,
                        StockQuantity = 20,
                        ShortDescription = "Thiết kế siêu mỏng kỷ lục 5.3mm, chip Apple M4 bứt phá mọi giới hạn đồ họa, màn hình Ultra Retina XDR Tandem OLED.",
                        Description = "iPad Pro M4 tái định nghĩa dòng máy tính bảng với độ mỏng khó tin, màn hình công nghệ đột phá Tandem OLED cho độ sáng cực đỉnh 1600 nits, hỗ trợ bút Apple Pencil Pro mang đến nguồn cảm hứng sáng tạo vô tận.",
                        MainImageUrl = "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Màn hình\":\"11 inch Tandem OLED Ultra Retina XDR\",\"Chip CPU\":\"Apple M4 9 nhân CPU, 10 nhân GPU\",\"Bộ nhớ trong\":\"256 GB\",\"Trọng lượng\":\"444 gram\"}",
                        IsFeatured = false,
                        IsBestSeller = false,
                        IsNew = true,
                        RatingAverage = 5.0,
                        ReviewCount = 15,
                        ViewCount = 450
                    },

                    // Laptops
                    new Product
                    {
                        Name = "MacBook Pro 14 inch M3 Pro (18GB / 512GB) Space Black",
                        Slug = "macbook-pro-14-inch-m3-pro-18gb-512gb",
                        Sku = "MBP14-M3P-18-512",
                        CategoryId = catLaptop.Id,
                        Price = 46990000,
                        OriginalPrice = 49990000,
                        StockQuantity = 18,
                        ShortDescription = "Màu đen không gian cuốn hút, chip M3 Pro cân mượt lập trình nặng và dựng video 8K, pin 22 giờ liên tục.",
                        Description = "MacBook Pro 14 inch sở hữu sức mạnh đột phá từ vi xử lý M3 Pro tiến trình 3nm, hỗ trợ Ray Tracing phần cứng và bộ nhớ hợp nhất 18GB cực nhanh. Màn hình Liquid Retina XDR hiển thị chuẩn xác từng sắc độ màu cho nhà sáng tạo nội dung.",
                        MainImageUrl = "https://images.unsplash.com/photo-1517336714731-489689fd1ca8?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Màn hình\":\"14.2 inch Liquid Retina XDR 120Hz ProMotion\",\"Chip CPU\":\"Apple M3 Pro 11-core CPU, 14-core GPU\",\"RAM\":\"18 GB Unified Memory\",\"Ổ cứng\":\"512 GB SSD siêu tốc\",\"Thời lượng pin\":\"Lên đến 22 giờ\"}",
                        IsFeatured = true,
                        IsBestSeller = true,
                        IsNew = true,
                        RatingAverage = 4.9,
                        ReviewCount = 42,
                        ViewCount = 890
                    },
                    new Product
                    {
                        Name = "Laptop ASUS ROG Zephyrus G16 OLED (i9-14900HX / RTX 4070 / 32GB / 1TB)",
                        Slug = "asus-rog-zephyrus-g16-oled-rtx4070",
                        Sku = "ASUS-G16-RTX4070",
                        CategoryId = catLaptop.Id,
                        Price = 52990000,
                        OriginalPrice = 58990000,
                        StockQuantity = 12,
                        ShortDescription = "Laptop Gaming siêu mỏng vỏ nhôm CNC nguyên khối, màn hình ROG Nebula OLED 240Hz, card đồ hoạ RTX 4070 mạnh mẽ.",
                        Description = "ROG Zephyrus G16 định hình lại chuẩn mực laptop gaming sang trọng. Màn hình OLED chuẩn điện ảnh 2.5K 240Hz phản hồi 0.2ms kết hợp cùng dải đèn Slash Lighting độc bản ở mặt A tạo nên phong cách tương lai đậm chất game thủ.",
                        MainImageUrl = "https://images.unsplash.com/photo-1603302576837-37561b2e2302?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Màn hình\":\"16 inch OLED 2.5K 240Hz 0.2ms 100% DCI-P3\",\"Chip CPU\":\"Intel Core Ultra 9 185H\",\"Card đồ họa\":\"NVIDIA GeForce RTX 4070 8GB GDDR6\",\"RAM\":\"32 GB LPDDR5X 7467MHz\",\"Ổ cứng\":\"1 TB PCIe 4.0 NVMe SSD\"}",
                        IsFeatured = true,
                        IsBestSeller = false,
                        IsNew = true,
                        RatingAverage = 4.9,
                        ReviewCount = 28,
                        ViewCount = 670
                    },
                    new Product
                    {
                        Name = "Laptop Dell XPS 13 9340 (Intel Core Ultra 7 / 16GB / 512GB SSD)",
                        Slug = "dell-xps-13-9340-intel-core-ultra-7",
                        Sku = "DELL-XPS13-9340",
                        CategoryId = catLaptop.Id,
                        Price = 38990000,
                        OriginalPrice = 42990000,
                        StockQuantity = 15,
                        ShortDescription = "Kiệt tác thiết kế tối giản không viền, bàn phím liền mạch chạm cảm ứng, siêu nhẹ chỉ 1.19kg.",
                        Description = "Dell XPS 13 thế hệ mới với hàng phím chức năng cảm ứng LED, trackpad tàng hình bằng kính liền mạch và viền màn hình siêu mỏng InfinityEdge. Hiệu năng đỉnh cao tích hợp NPU AI sẵn sàng cho kỷ nguyên Copilot+ PC.",
                        MainImageUrl = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Màn hình\":\"13.4 inch FHD+ InfinityEdge 500 nits 120Hz\",\"Chip CPU\":\"Intel Core Ultra 7 155H 16 nhân\",\"RAM\":\"16 GB LPDDR5x\",\"Ổ cứng\":\"512 GB SSD M.2 PCIe Gen 4\",\"Trọng lượng\":\"1.19 kg\"}",
                        IsFeatured = false,
                        IsBestSeller = false,
                        IsNew = true,
                        RatingAverage = 4.7,
                        ReviewCount = 19,
                        ViewCount = 520
                    },

                    // Audio
                    new Product
                    {
                        Name = "Tai nghe Sony WH-1000XM5 Chống Ồn Cao Cấp Đen",
                        Slug = "tai-nghe-sony-wh-1000xm5-den",
                        Sku = "SONY-WH1000XM5-BLK",
                        CategoryId = catAudio.Id,
                        Price = 7490000,
                        OriginalPrice = 8990000,
                        ShortDescription = "Khử tiếng ồn đỉnh cao hàng đầu thế giới với 2 chip xử lý và 8 mic, chất âm Hi-Res Audio không dây, pin 30 giờ.",
                        Description = "Sony WH-1000XM5 mang đến trải nghiệm âm thanh không tiếng ồn chân thực nhất hiện nay nhờ vi xử lý tích hợp V1 và bộ xử lý QN1 chuyên biệt. Củ loa 30mm màng carbon nhẹ và cứng tái hiện âm cao tinh khiết và dải trầm sâu lắng.",
                        MainImageUrl = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Loại tai nghe\":\"Chụp tai Over-ear không dây\",\"Chống ồn\":\"Active Noise Cancelling (ANC) kép\",\"Thời lượng pin\":\"30 giờ (bật ANC), 40 giờ (tắt ANC)\",\"Sạc nhanh\":\"3 phút sạc cho 3 giờ phát nhạc\",\"Chuẩn âm thanh\":\"Hi-Res Audio Wireless, LDAC, DSEE Extreme\"}",
                        StockQuantity = 50,
                        IsFeatured = true,
                        IsBestSeller = true,
                        IsNew = false,
                        RatingAverage = 4.9,
                        ReviewCount = 120,
                        ViewCount = 1420
                    },
                    new Product
                    {
                        Name = "Tai nghe Apple AirPods Pro 2 MagSafe USB-C",
                        Slug = "tai-nghe-apple-airpods-pro-2-usbc",
                        Sku = "APP2-USBC",
                        CategoryId = catAudio.Id,
                        Price = 5490000,
                        OriginalPrice = 6190000,
                        ShortDescription = "Chip H2 nâng cấp chống ồn gấp đôi, cổng sạc USB-C hiện đại, chống bụi nước chuẩn IP54.",
                        Description = "AirPods Pro 2 trang bị chip Apple H2 cho khả năng khử tiếng ồn chủ động gấp 2 lần, chế độ Thích ứng âm thanh (Adaptive Audio) thông minh chuyển đổi mượt mà theo môi trường thực tế và Spatial Audio cá nhân hoá sống động.",
                        MainImageUrl = "https://images.unsplash.com/photo-1600294037681-c80b4cb5b434?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Kết nối\":\"Bluetooth 5.3\",\"Chip\":\"Apple H2 trong tai nghe, Apple U1 trong hộp sạc\",\"Thời lượng pin\":\"6 giờ nghe liên tục, 30 giờ kèm hộp sạc\",\"Chuẩn kháng nước\":\"IP54 chống nước mồ hôi và bụi bẩn\"}",
                        StockQuantity = 60,
                        IsFeatured = true,
                        IsBestSeller = true,
                        IsNew = false,
                        RatingAverage = 4.8,
                        ReviewCount = 210,
                        ViewCount = 2100
                    },
                    new Product
                    {
                        Name = "Loa Bluetooth Marshall Stanmore III Đen Chính Hãng",
                        Slug = "loa-bluetooth-marshall-stanmore-iii-den",
                        Sku = "MARSHALL-STAN3-BLK",
                        CategoryId = catAudio.Id,
                        Price = 8990000,
                        OriginalPrice = 9990000,
                        ShortDescription = "Thiết kế Vintage đậm chất Rock'n'roll, âm trường rộng mở Dynamic Loudness, công suất 80W uy lực.",
                        Description = "Marshall Stanmore III là mẫu loa để bàn biểu tượng lấp đầy mọi không gian phòng khách bằng âm thanh chân thực sống động. Mặt trước bọc lưới kim loại viền đồng cổ điển, tích hợp công nghệ Bluetooth 5.2 kết nối tức thì không độ trễ.",
                        MainImageUrl = "https://images.unsplash.com/photo-1545454675-3531b543be5d?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Công suất tổng\":\"80W (1x50W Woofer + 2x15W Tweeter)\",\"Kết nối\":\"Bluetooth 5.2, AUX 3.5mm, RCA\",\"Dải tần đáp ứng\":\"45 - 20,000 Hz\",\"Kích thước\":\"350 x 203 x 188 mm, Nặng 4.25 kg\"}",
                        StockQuantity = 25,
                        IsFeatured = false,
                        IsBestSeller = true,
                        IsNew = false,
                        RatingAverage = 4.9,
                        ReviewCount = 45,
                        ViewCount = 760
                    },

                    // Smartwatches
                    new Product
                    {
                        Name = "Apple Watch Ultra 2 GPS + Cellular 49mm Dây Alpine Vòng Cam",
                        Slug = "apple-watch-ultra-2-gps-cellular-49mm",
                        Sku = "AW-ULTRA2-49-ALP",
                        CategoryId = catWatch.Id,
                        Price = 19990000,
                        OriginalPrice = 21990000,
                        ShortDescription = "Vỏ titan hàng không 49mm, màn hình sáng 3000 nits, chuẩn lặn EN13319 sâu 40m, GPS tần số kép chuẩn xác.",
                        Description = "Được sinh ra cho những nhà thám hiểm và vận động viên thể thao khắc nghiệt, Apple Watch Ultra 2 sở hữu chip S9 SiP tiên tiến với cử chỉ chạm hai lần (Double Tap) kỳ diệu. Thời lượng pin lên đến 72 giờ ở chế độ tiết kiệm năng lượng.",
                        MainImageUrl = "https://images.unsplash.com/photo-1508685096489-7aacd43bd3b1?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Kích thước mặt\":\"49mm Titan\",\"Độ sáng màn hình\":\"3000 nits cao nhất lịch sử Apple\",\"Chống nước\":\"Chống nước 100m, lặn sâu 40m\",\"Thời lượng pin\":\"36 giờ dùng thông thường, 72 giờ tiết kiệm pin\"}",
                        StockQuantity = 22,
                        IsFeatured = true,
                        IsBestSeller = false,
                        IsNew = true,
                        RatingAverage = 4.9,
                        ReviewCount = 37,
                        ViewCount = 640
                    },
                    new Product
                    {
                        Name = "Đồng hồ thông minh Garmin Fenix 7 Pro Solar Edition Đen",
                        Slug = "garmin-fenix-7-pro-solar-edition-den",
                        Sku = "GARMIN-FENIX7-PRO",
                        CategoryId = catWatch.Id,
                        Price = 17990000,
                        OriginalPrice = 19490000,
                        ShortDescription = "Mặt kính sạc năng lượng mặt trời Power Glass, đèn pin LED tích hợp, thời lượng pin 22 ngày vượt trội.",
                        Description = "Garmin Fenix 7 Pro Solar là chiến hữu tối thượng cho những ai đam mê chạy trail, ba môn phối hợp và leo núi. Bản đồ địa hình Topo đa lục địa chi tiết, cảm biến nhịp tim thế hệ thứ 5 mới nhất đo lường chính xác từng nhịp đập.",
                        MainImageUrl = "https://images.unsplash.com/photo-1579586337278-3befd40fd17a?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Màn hình\":\"1.3 inch MIP chống chói Sunlight-visible\",\"Pin\":\"Lên đến 22 ngày kèm sạc năng lượng mặt trời\",\"Bản đồ\":\"Bản đồ màu TopoActive tải sẵn\",\"Cảm biến\":\"Elevate Gen 5, Pulse Ox, GPS đa băng tần\"}",
                        StockQuantity = 14,
                        IsFeatured = false,
                        IsBestSeller = false,
                        IsNew = false,
                        RatingAverage = 4.8,
                        ReviewCount = 29,
                        ViewCount = 410
                    },

                    // Accessories
                    new Product
                    {
                        Name = "Củ sạc nhanh Anker Prime 67W GaN 3 cổng (2C1A)",
                        Slug = "cu-sac-nhanh-anker-prime-67w-gan-3-cong",
                        Sku = "ANKER-A2669-BLK",
                        CategoryId = catAcc.Id,
                        Price = 990000,
                        OriginalPrice = 1350000,
                        ShortDescription = "Công nghệ GaNPrime thế hệ mới nhỏ gọn hơn 51%, kiểm soát nhiệt ActiveShield 2.0, công suất sạc 67W cho MacBook.",
                        Description = "Củ sạc Anker Prime 67W hỗ trợ sạc cùng lúc 3 thiết bị với phân bổ công suất thông minh PowerIQ 4.0. Thiết kế chân cắm gập tiện lợi mang theo du lịch, an toàn bảo vệ ngắn mạch và quá dòng tuyệt đối.",
                        MainImageUrl = "https://images.unsplash.com/photo-1583863788434-e58a36330cf0?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Tổng công suất\":\"67W Max\",\"Cổng kết nối\":\"2 cổng USB-C + 1 cổng USB-A\",\"Công nghệ\":\"GaNPrime, PowerIQ 4.0, ActiveShield 2.0\",\"Trọng lượng\":\"135 gram\"}",
                        StockQuantity = 120,
                        IsFeatured = false,
                        IsBestSeller = true,
                        IsNew = false,
                        RatingAverage = 4.9,
                        ReviewCount = 310,
                        ViewCount = 1800
                    },
                    new Product
                    {
                        Name = "Bàn phím cơ không dây Keychron K3 Pro QMK/VIA Switch Red",
                        Slug = "ban-phim-co-keychron-k3-pro-red-switch",
                        Sku = "KC-K3PRO-RED",
                        CategoryId = catAcc.Id,
                        Price = 2190000,
                        OriginalPrice = 2490000,
                        ShortDescription = "Bàn phím cơ Low Profile siêu mỏng nhẹ, hỗ trợ tùy biến layout QMK/VIA, kết nối Bluetooth 5.1 và cáp Type-C.",
                        Description = "Keychron K3 Pro là bàn phím cơ công thái học hoàn hảo cho góc làm việc hiện đại. Tương thích tối ưu cho cả macOS và Windows, switch Gateron Low Profile êm ái cùng hệ thống đèn LED RGB hơn 22 chế độ chuyển sắc mượt mà.",
                        MainImageUrl = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Layout\":\"75% 84 phím bấm\",\"Switch\":\"Gateron Low Profile Red Switch (Hot-swap)\",\"Kết nối\":\"Bluetooth 5.1 & Type-C có dây\",\"Tương thích\":\"macOS / Windows / Linux / Android\"}",
                        StockQuantity = 40,
                        IsFeatured = true,
                        IsBestSeller = false,
                        IsNew = false,
                        RatingAverage = 4.8,
                        ReviewCount = 76,
                        ViewCount = 890
                    },
                    new Product
                    {
                        Name = "Chuột không dây Logitech MX Master 3S For Mac Space Gray",
                        Slug = "chuot-logitech-mx-master-3s-for-mac",
                        Sku = "LOGI-MX3S-MAC",
                        CategoryId = catAcc.Id,
                        Price = 2290000,
                        OriginalPrice = 2690000,
                        ShortDescription = "Cảm biến 8.000 DPI lướt mượt trên mọi bề mặt kính, con cuộn MagSpeed 1000 dòng/giây, click tĩnh âm Quiet Clicks.",
                        Description = "Logitech MX Master 3S là biểu tượng chuột văn phòng chuyên nghiệp dành cho lập trình viên và nhà thiết kế. Phom cầm công thái học hoàn hảo ôm trọn bàn tay cùng phần mềm Logi Options+ tuỳ biến phím tắt theo từng ứng dụng.",
                        MainImageUrl = "https://images.unsplash.com/photo-1615663245857-ac93bb7c39e7?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Độ phân giải cảm biến\":\"200 - 8000 DPI điều chỉnh được\",\"Số nút bấm\":\"7 nút bấm\",\"Thời lượng pin\":\"Lên đến 70 ngày sau 1 lần sạc đầy\",\"Kết nối\":\"Bluetooth Low Energy, Logi Bolt USB\"}",
                        StockQuantity = 55,
                        IsFeatured = false,
                        IsBestSeller = true,
                        IsNew = false,
                        RatingAverage = 4.9,
                        ReviewCount = 145,
                        ViewCount = 1350
                    },

                    // Smart Home
                    new Product
                    {
                        Name = "Robot hút bụi lau nhà Dreame L20 Ultra Chính Hãng",
                        Slug = "robot-hut-bui-dreame-l20-ultra",
                        Sku = "DREAME-L20U-WHT",
                        CategoryId = catHome.Id,
                        Price = 18990000,
                        OriginalPrice = 23990000,
                        ShortDescription = "Lực hút cực đại 7000Pa, công nghệ xòe giẻ lau MopExtend sát mép chân tường, trạm sạc toàn năng tự giặt sấy sấy khô.",
                        Description = "Dreame L20 Ultra giải phóng hoàn toàn sức lao động với trạm sạc thông minh: tự thêm nước lau sàn, tự động tháo giẻ khi gặp thảm dày và camera AI nhận diện né tránh hơn 55 loại vật cản trên sàn nhà.",
                        MainImageUrl = "https://images.unsplash.com/photo-1628177142898-93e36e4e3a50?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Lực hút\":\"7000 Pa Vormax\",\"Dung lượng pin\":\"6400 mAh\",\"Dung tích hộp bụi\":\"300 ml (hộp rác robot), túi rác 3.2L trong trạm\",\"Tính năng trạm sạc\":\"Tự đổ rác, tự giặt sấy giẻ bằng khí nóng 45°C, tự bơm nước\"}",
                        StockQuantity = 15,
                        IsFeatured = true,
                        IsBestSeller = false,
                        IsNew = true,
                        RatingAverage = 4.9,
                        ReviewCount = 33,
                        ViewCount = 590
                    },
                    new Product
                    {
                        Name = "Camera an ninh xoay 360 độ Xiaomi Smart Camera C400 2.5K",
                        Slug = "camera-xiaomi-smart-camera-c400-2k",
                        Sku = "MI-CAM-C400",
                        CategoryId = catHome.Id,
                        Price = 890000,
                        OriginalPrice = 1090000,
                        ShortDescription = "Hình ảnh siêu sắc nét 2.5K 4MP, xoay góc ngang 360 độ dọc 106 độ, AI phát hiện chuyển động người.",
                        Description = "Xiaomi Smart Camera C400 bảo vệ an ninh ngôi nhà 24/7 với đèn hồng ngoại nhìn đêm rõ ràng không phát ánh sáng đỏ gây chói mắt. Đàm thoại 2 chiều qua micro khử tiếng ồn và điều khiển dễ dàng trên app Mi Home.",
                        MainImageUrl = "https://images.unsplash.com/photo-1557324232-b8917d3c3dcb?w=800&auto=format&fit=crop&q=80",
                        SpecificationsJson = "{\"Độ phân giải\":\"2560 x 1440 (4 Megapixel)\",\"Góc quay\":\"360° ngang, 106° dọc\",\"Lưu trữ\":\"Thẻ nhớ MicroSD tối đa 256GB hoặc lưu đám mây\",\"Đàm thoại\":\"Hai chiều thời gian thực\"}",
                        StockQuantity = 80,
                        IsFeatured = false,
                        IsBestSeller = true,
                        IsNew = false,
                        RatingAverage = 4.7,
                        ReviewCount = 92,
                        ViewCount = 820
                    }
                };

                await context.Products.AddRangeAsync(products);
                await context.SaveChangesAsync();

                // Add secondary images for products
                foreach (var prod in products)
                {
                    context.ProductImages.Add(new ProductImage
                    {
                        ProductId = prod.Id,
                        ImageUrl = prod.MainImageUrl ?? "",
                        DisplayOrder = 1
                    });
                }
                await context.SaveChangesAsync();
            }

            // 6. Seed Coupons
            if (!await context.Coupons.AnyAsync())
            {
                var coupons = new List<Coupon>
                {
                    new Coupon
                    {
                        Code = "GIAM10",
                        Description = "Giảm 10% cho tất cả đơn hàng từ 500.000đ",
                        Type = CouponType.Percentage,
                        DiscountValue = 10,
                        MinimumSpend = 500000,
                        ExpiryDate = DateTime.UtcNow.AddMonths(3),
                        IsActive = true
                    },
                    new Coupon
                    {
                        Code = "FREESHIP",
                        Description = "Miễn phí vận chuyển (giảm 30.000đ)",
                        Type = CouponType.FixedAmount,
                        DiscountValue = 30000,
                        MinimumSpend = 300000,
                        ExpiryDate = DateTime.UtcNow.AddMonths(3),
                        IsActive = true
                    },
                    new Coupon
                    {
                        Code = "WELCOME50K",
                        Description = "Giảm trực tiếp 50.000đ cho khách hàng mới",
                        Type = CouponType.FixedAmount,
                        DiscountValue = 50000,
                        MinimumSpend = 1000000,
                        ExpiryDate = DateTime.UtcNow.AddMonths(6),
                        IsActive = true
                    }
                };

                await context.Coupons.AddRangeAsync(coupons);
                await context.SaveChangesAsync();
            }

            // 7. Seed Sample Orders for Customer
            if (!await context.Orders.AnyAsync() && customerUser != null)
            {
                var firstProduct = await context.Products.FirstAsync();
                var secondProduct = await context.Products.Skip(1).FirstAsync();

                var order = new Order
                {
                    OrderCode = "ORD-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-8891",
                    UserId = customerUser.Id,
                    CustomerName = customerUser.FullName,
                    CustomerEmail = customerUser.Email!,
                    CustomerPhone = customerUser.PhoneNumber ?? "0987654321",
                    ProvinceCity = "Hà Nội",
                    WardDistrict = "Quận Cầu Giấy, Phường Dịch Vọng",
                    ShippingAddress = "123 Cầu Giấy",
                    OrderNotes = "Giao hàng giờ hành chính giúp tôi.",
                    SubTotal = firstProduct.Price + secondProduct.Price,
                    ShippingFee = 30000,
                    DiscountAmount = 0,
                    TotalAmount = firstProduct.Price + secondProduct.Price + 30000,
                    PaymentMethod = PaymentMethod.COD,
                    PaymentStatus = PaymentStatus.Unpaid,
                    Status = OrderStatus.Preparing,
                    TrackingNumber = "VNPOST-99281923",
                    CreatedAt = DateTime.UtcNow.AddDays(-1),
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = firstProduct.Id,
                            ProductName = firstProduct.Name,
                            ProductImageUrl = firstProduct.MainImageUrl,
                            Quantity = 1,
                            UnitPrice = firstProduct.Price,
                            TotalPrice = firstProduct.Price
                        },
                        new OrderItem
                        {
                            ProductId = secondProduct.Id,
                            ProductName = secondProduct.Name,
                            ProductImageUrl = secondProduct.MainImageUrl,
                            Quantity = 1,
                            UnitPrice = secondProduct.Price,
                            TotalPrice = secondProduct.Price
                        }
                    }
                };

                var deliveredOrder = new Order
                {
                    OrderCode = "ORD-" + DateTime.UtcNow.AddDays(-10).ToString("yyyyMMdd") + "-2104",
                    UserId = customerUser.Id,
                    CustomerName = customerUser.FullName,
                    CustomerEmail = customerUser.Email!,
                    CustomerPhone = customerUser.PhoneNumber ?? "0987654321",
                    ProvinceCity = "Hà Nội",
                    WardDistrict = "Quận Cầu Giấy, Phường Dịch Vọng",
                    ShippingAddress = "123 Cầu Giấy",
                    OrderNotes = "Gọi trước khi giao",
                    SubTotal = 990000,
                    ShippingFee = 0,
                    DiscountAmount = 30000,
                    TotalAmount = 960000,
                    PaymentMethod = PaymentMethod.COD,
                    PaymentStatus = PaymentStatus.Paid,
                    Status = OrderStatus.Delivered,
                    TrackingNumber = "GHN-8827101",
                    CreatedAt = DateTime.UtcNow.AddDays(-10),
                    Items = new List<OrderItem>
                    {
                        new OrderItem
                        {
                            ProductId = null,
                            ProductName = "Củ sạc nhanh Anker Prime 67W GaN 3 cổng (2C1A)",
                            ProductImageUrl = "https://images.unsplash.com/photo-1583863788434-e58a36330cf0?w=800&auto=format&fit=crop&q=80",
                            Quantity = 1,
                            UnitPrice = 990000,
                            TotalPrice = 990000
                        }
                    }
                };

                context.Orders.AddRange(order, deliveredOrder);
                await context.SaveChangesAsync();
            }
        }
    }
}
