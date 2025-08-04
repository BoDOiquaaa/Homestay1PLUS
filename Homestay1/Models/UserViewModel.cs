using System.ComponentModel.DataAnnotations;

namespace Homestay1.ViewModels
{
    public class UserViewModel
    {
        public int? UserID { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public int RoleID { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, ErrorMessage = "Họ tên không được quá 100 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(150, ErrorMessage = "Email không được quá 150 ký tự")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        public string? Phone { get; set; }

        // Chỉ dùng khi tạo mới user
        public string? Password { get; set; }
    }
}