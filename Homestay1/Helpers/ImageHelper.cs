// Helpers/ImageHelper.cs (Tùy chọn - để code sạch hơn)
namespace Homestay1.Helpers
{
    public static class ImageHelper
    {
        public static string GetHomestayImagePath(string? imageUrl)
        {
            return !string.IsNullOrEmpty(imageUrl)
                ? $"~/Img/{imageUrl}"
                : "~/Img/default-homestay.jpg";
        }

        public static string GetRoomImagePath(string? imageUrl)
        {
            return !string.IsNullOrEmpty(imageUrl)
                ? $"~/Img/{imageUrl}"
                : "~/Img/default-room.jpg";
        }

        // Tự động tạo tên file từ ID
        public static string GenerateHomestayImageName(int homestayId, string extension = "jpg")
        {
            return $"homestay{homestayId}.{extension}";
        }

        public static string GenerateRoomImageName(int roomId, string extension = "jpg")
        {
            return $"room{roomId}.{extension}";
        }
    }
}