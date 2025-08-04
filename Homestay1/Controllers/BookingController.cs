using Homestay1.Data;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;

namespace Homestay1.Controllers
{
    public class BookingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public BookingController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Create()
        {
            var rooms = _context.Rooms
                .Where(r => r.Status == "Available")
                .Select(r => new { r.RoomID, r.RoomName })
                .ToList();

            ViewBag.Rooms = new SelectList(rooms, "RoomID", "RoomName");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(Homestay1.Models.Booking model)
        {
            if (ModelState.IsValid)
            {
                // Lưu booking vào DB
                _context.SaveChanges();

                ViewBag.Message = "Đặt phòng thành công!";
                // Nếu muốn chuyển sang trang khác, dùng RedirectToAction
                // return RedirectToAction("Index", "Home");
            }

            // Nếu không hợp lệ hoặc lỗi, load lại danh sách phòng để hiển thị dropdown
            var rooms = _context.Rooms
                .Where(r => r.Status == "Available")
                .Select(r => new { r.RoomID, r.RoomName })
                .ToList();

            ViewBag.Rooms = new SelectList(rooms, "RoomID", "RoomName");
            return View(model);
        }
    }
}
