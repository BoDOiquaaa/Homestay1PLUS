using Microsoft.AspNetCore.Mvc.Rendering;

namespace Homestay1.Models
{
    public class BookingViewModel
    {
        public string Name { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public int Guests { get; set; }

        public int? SelectedRoomId { get; set; }
        public IEnumerable<SelectListItem>? RoomOptions { get; set; }
    }



}
