namespace Homestay1.Models
{
    public class Room
    {
        public int RoomID { get; set; }
        public int HomestayID { get; set; }
        public string RoomName { get; set; }
        public decimal PricePerNight { get; set; }
        public string Status { get; set; }
        public string ImageUrl { get; set; }

        public Homestay Homestay { get; set; }
    }
}