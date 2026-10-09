# TechZone - Nền Tảng Thương Mại Điện Tử Full-Stack ASP.NET Core & SQL Server

Hệ thống website thương mại điện tử hiện đại, chuyên nghiệp, responsive được xây dựng hoàn chỉnh từ Backend tới Frontend theo mô hình MVC kết hợp Service Layer và Entity Framework Core Code-First trên nền tảng .NET 8.

---

## 1. Công Nghệ Sử Dụng

### Backend
- **Framework**: ASP.NET Core 8.0 MVC (C#)
- **Cơ sở dữ liệu**: Microsoft SQL Server Express LocalDB (`MSSQLLocalDB`)
- **ORM**: Entity Framework Core 8.0 (Code-First Migrations)
- **Bảo mật & Phân quyền**: ASP.NET Core Identity (Role-based: `Admin`, `Customer`)
- **Session & Caching**: Memory Cache, Cookies Session bền vững

### Frontend
- **Giao diện**: HTML5, CSS3 hiện đại, Vanilla JavaScript
- **CSS Framework**: Bootstrap 5.3 + Bootstrap Icons
- **Hiệu ứng & Trải nghiệm**: Responsive Design, Skeleton Loaders, Floating Toasts, Micro-animations
- **Biểu đồ**: Chart.js 4.4 cho trang Admin Dashboard

---

## 2. Tài Khoản Thử Nghiệm Sẵn Có

Hệ thống tự động khởi tạo dữ liệu mẫu (Seeding) khi chạy lần đầu:

| Vai trò | Email đăng nhập | Mật khẩu | Chức năng chính |
| :--- | :--- | :--- | :--- |
| **Quản trị viên (Admin)** | `admin@eshop.com` | `Admin@123456` | Toàn quyền Dashboard, CRUD Sản phẩm, Quản lý Đơn hàng, Quản lý User |
| **Khách hàng (Customer)** | `user@eshop.com` | `User@123456` | Xem giỏ hàng, Lịch sử mua hàng, Sổ địa chỉ, Đổi mật khẩu |

> **Ghi chú**: Tại trang Đăng nhập (`/Account/Login`), hệ thống đã tích hợp sẵn 2 nút bấm **"Admin"** và **"Khách hàng"** để tự động điền tài khoản thử nghiệm nhanh chóng chỉ với 1 click!

---

## 3. Mã Giảm Giá Khuyến Mãi Sẵn Có

Khi thanh toán đơn hàng, bạn có thể nhập các mã sau để kiểm tra tính năng giảm giá:
- `GIAM10`: Giảm 10% tổng giá trị đơn hàng
- `FREESHIP`: Miễn phí vận chuyển (giảm 30.000đ)
- `WELCOME50K`: Giảm trực tiếp 50.000đ cho đơn từ 1.000.000đ

---

## 4. Hướng Dẫn Cấu Hình Và Chạy Trong Visual Studio

### Yêu cầu môi trường
- **Visual Studio 2022** (bản 17.8 trở lên) có cài đặt workload **"ASP.NET and web development"**.
- **.NET 8.0 SDK** (đã được cài đặt sẵn).
- **SQL Server Express LocalDB** (đi kèm sẵn trong Visual Studio).

### Các bước mở và chạy dự án:
1. Mở file giải pháp **`MyWebShop.sln`** bằng Visual Studio 2022.
2. Kiểm tra chuỗi kết nối trong file `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=MyWebShopDb;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
   }
   ```
3. Nhấn **F5** hoặc nút **"Play (http / https)"** trên thanh công cụ để khởi động dự án.
4. Trình duyệt sẽ tự động mở trang web tại địa chỉ: `http://localhost:5071/` hoặc `https://localhost:7212/`.
5. Trong lần đầu khởi chạy, `DbInitializer` sẽ tự động tạo bảng và nạp 16 sản phẩm công nghệ, 6 danh mục, mã giảm giá và 2 tài khoản mẫu.

### Chạy bằng dòng lệnh (Terminal / CLI):
```powershell
cd c:\Users\Administrator\Desktop\MyWeb
dotnet run --launch-profile http
```
Truy cập: `http://localhost:5071`

---

## 5. Danh Sách Các Chức Năng Đã Triển Khai Hoàn Chỉnh

### 1. Hệ thống Xác thực & Người dùng (Identity)
- **Đăng ký tài khoản**: Kiểm tra trùng email, kiểm tra độ mạnh mật khẩu, xác nhận mật khẩu, đồng ý điều khoản, thông báo Toast.
- **Đăng nhập**: Phân quyền Admin và Khách hàng, tự động ghi nhớ phiên đăng nhập, khóa tạm thời tài khoản nếu đăng nhập sai quá 5 lần.
- **Đăng xuất**: Xóa phiên an toàn.
- **Quên & Đặt lại mật khẩu**: Sinh token bảo mật thời hạn ngắn, hỗ trợ đổi mật khẩu mới.
- **Hồ sơ cá nhân**: Xem thông tin, cập nhật họ tên & số điện thoại, sổ địa chỉ giao hàng, đổi mật khẩu.

### 2. Trang chủ & Danh mục sản phẩm
- **Hero Banner**: Slider quảng cáo sản phẩm tâm điểm.
- **Danh mục icon**: 6 danh mục chính hãng (Điện thoại & Tablet, Laptop & Máy tính, Tai nghe & Âm thanh, Đồng hồ thông minh, Phụ kiện, Smart Home).
- **Phân loại sản phẩm**: Giảm giá sốc (Flash sale), Nổi bật (Featured), Bán chạy (Best sellers), Hàng mới về (New arrivals).
- **Thẻ sản phẩm chuẩn E-Commerce**: Ảnh zoom khi hover, nhãn giảm giá `-X%`, trạng thái còn hàng, đánh giá sao, nút "Thêm vào giỏ" (AJAX) và nút "Mua ngay".

### 3. Trang Danh mục & Chi tiết sản phẩm
- **Bộ lọc sản phẩm**: Lọc theo danh mục, lọc theo khoảng giá từ - đến, lọc sản phẩm còn hàng.
- **Sắp xếp**: Giá tăng/giảm dần, mới nhất, nổi bật, đánh giá cao, xem nhiều nhất.
- **Phân trang & Đếm số kết quả**: Phân trang chuẩn, hiển thị tổng số sản phẩm.
- **Chi tiết sản phẩm**: Bộ sưu tập ảnh (chuyển đổi ảnh tức thì), mã SKU, giá khuyến mãi, bảng thông số kỹ thuật chi tiết, đánh giá của người mua, sản phẩm tương tự.

### 4. Hệ thống Giỏ hàng (Shopping Cart)
- **Thêm vào giỏ không tải lại trang (AJAX)**: Cập nhật huy hiệu số lượng trên Header, hiển thị Toast thông báo.
- **Hỗ trợ cả khách vãng lai và thành viên**: Giỏ hàng lưu qua Cookie Session và tự động gộp (merge) vào tài khoản khi người dùng đăng nhập.
- **Trang Giỏ hàng tương tác**: Nút tăng/giảm số lượng với cập nhật tính tiền tức thì, xóa sản phẩm, xóa toàn bộ giỏ, kiểm tra tồn kho tối đa.

### 5. Đặt hàng & Thanh toán (Checkout & Orders)
- **Mua ngay**: Nhấn mua ngay chuyển thẳng đến trang điền thông tin đơn hàng mà không cần thêm vào giỏ.
- **Form thanh toán**: Họ tên, email, SĐT, Tỉnh/thành phố, Quận/huyện, Địa chỉ chi tiết, Ghi chú giao hàng.
- **Phương thức thanh toán**:
  - Thanh toán khi nhận hàng (COD) - trạng thái "Chưa thanh toán".
  - Chuyển khoản ngân hàng (Hiển thị thông tin STK và hướng dẫn chuyển khoản).
- **Giao dịch Database an toàn (Transaction)**: Khóa trừ số lượng tồn kho tự động, ngăn chặn đặt hàng trùng lặp khi bấm nhiều lần, xóa giỏ hàng sau khi đặt thành công.
- **Trang thành công & Theo dõi đơn**: Hiển thị mã đơn `ORD-YYYYMMDD-XXXX`, tiến trình giao hàng theo dòng thời gian (Timeline) và chức năng hủy đơn hàng nếu còn ở trạng thái Chờ xác nhận.

### 6. Trang Quản Trị Hệ Thống (Admin Panel `/Admin`)
- **Dashboard**:
  - Thống kê: Doanh thu thực, Tổng đơn hàng, Đơn chờ xử lý, Tổng khách hàng, Cảnh báo sắp hết hàng.
  - **Chart.js**: Biểu đồ cột xu hướng doanh thu 6 tháng & Biểu đồ tròn tỷ lệ trạng thái đơn hàng.
  - Danh sách đơn hàng mới nhất và danh sách sản phẩm sắp cạn kho.
- **Quản lý sản phẩm**: Thêm mới sản phẩm, sửa giá/kho/mô tả, ẩn sản phẩm, tìm kiếm & lọc theo danh mục.
- **Quản lý danh mục**: Tạo danh mục mới, chỉnh sửa icon và thứ tự hiển thị.
- **Quản lý đơn hàng**: Xem chi tiết đơn hàng, lọc theo trạng thái (Chờ xác nhận, Đã xác nhận, Đang chuẩn bị, Đang giao, Đã giao, Đã hủy), cập nhật trạng thái thanh toán và tự động hoàn trả kho nếu hủy đơn.
- **Quản lý người dùng**: Danh sách thành viên, phân quyền, chức năng khóa / mở khóa tài khoản người dùng an toàn.
