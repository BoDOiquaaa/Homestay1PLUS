using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Homestay1.Models.Entities
{
    public class User
    {
        [Required(ErrorMessage = "Vui lòng chọn vai trò")]
        public int UserID { get; set; }

        [Required]
        [Display(Name = "Vai trò")]
        public int RoleID { get; set; }

        [Required, StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }

        [Phone]
        public string Phone { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [ForeignKey(nameof(RoleID))]
        public Role Role { get; set; }
    }
}