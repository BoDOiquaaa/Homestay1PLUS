using System;
using System.ComponentModel.DataAnnotations;
namespace Homestay1.Models
{
    public class Booking
    {
        public int BookingID { get; set; }

        // Các thuộc tính hiện có
        public string Name { get; set; }  // nếu bạn dùng Name cho khách
        public DateTime CheckIn { get; set; }
        public DateTime CheckOut { get; set; }
        public int Guests { get; set; }

        // Thêm RoomID để lưu phòng khách chọn
        [Required(ErrorMessage = "Vui lòng chọn phòng")]
        public int RoomID { get; set; }

        // Nếu có navigation property
        public virtual Room Room { get; set; }
    }
}
