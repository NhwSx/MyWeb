using System.ComponentModel.DataAnnotations;

namespace MyWebShop.Models.Entities
{
    public class UserAddress
    {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;
        public virtual ApplicationUser? User { get; set; }

        [Required(ErrorMessage = "Họ tên người nhận không được để trống")]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tỉnh / Thành phố không được để trống")]
        [StringLength(100)]
        public string ProvinceCity { get; set; } = string.Empty;

        [Required(ErrorMessage = "Quận / Huyện / Phường / Xã không được để trống")]
        [StringLength(100)]
        public string WardDistrict { get; set; } = string.Empty;

        [Required(ErrorMessage = "Địa chỉ chi tiết không được để trống")]
        [StringLength(250)]
        public string StreetAddress { get; set; } = string.Empty;

        public bool IsDefault { get; set; } = false;
    }
}
