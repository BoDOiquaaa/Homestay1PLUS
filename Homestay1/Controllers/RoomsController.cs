using Homestay1.Models;
using Homestay1.Repositories;
using Homestay1.Repository;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.IO;
using System.Threading.Tasks;
using Homestay1.Filters;

namespace Homestay1.Areas.Ad.Controllers
{
    [Area("Ad")]
    [OwnerAuthorization]
    [Route("Ad/[controller]/[action]/{id?}")]
    public class RoomsController : Controller
    {
        private readonly IRoomRepository _roomRepo;
        private readonly IHomestayRepository _homestayRepo;

        public RoomsController(IRoomRepository roomRepo, IHomestayRepository homestayRepo)
        {
            _roomRepo = roomRepo;
            _homestayRepo = homestayRepo;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search)
        {
            ViewData["CurrentFilter"] = search;
            var rooms = await _roomRepo.GetAllAsync();

            if (!string.IsNullOrEmpty(search))
                rooms = rooms.Where(r => r.RoomName.Contains(search)).ToList();

      

            return View(rooms);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var homestays = await _homestayRepo.GetAllAsync();
            ViewData["Homestays"] = homestays;

            // Tạo token cho view
            ViewBag.RequestVerificationToken = HttpContext.RequestServices
                .GetService<Microsoft.AspNetCore.Antiforgery.IAntiforgery>()
                .GetAndStoreTokens(HttpContext).RequestToken;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAjaxWithImage(IFormCollection form, IFormFile ImageFile)
        {
            try
            {
                var room = new Room
                {
                    HomestayID = int.Parse(form["HomestayID"]),
                    RoomName = form["RoomName"],
                    PricePerNight = decimal.Parse(form["PricePerNight"]),
                    Status = form["Status"]
                };

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/rooms");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    room.ImageUrl = "/images/rooms/" + fileName;
                }

                await _roomRepo.AddAsync(room);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var room = await _roomRepo.GetByIdAsync(id);
                if (room == null)
                    return NotFound();

                // LỖI CHÍNH Ở ĐÂY - SỬA LẠI
                var homestays = await _homestayRepo.GetAllAsync();

                // Kiểm tra homestays có null không
                if (homestays == null || !homestays.Any())
                {
                    // Nếu không có data, tạo list rỗng
                    ViewBag.Homestays = new SelectList(new List<object>(), "HomestayID", "Name");
                }
                else
                {
                    // SỬA: Dùng ViewBag.Homestays thay vì ViewData["HomestayID"]
                    ViewBag.Homestays = new SelectList(homestays, "HomestayID", "Name", room.HomestayID);
                }

                return View(room);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu cần
                ViewBag.Homestays = new SelectList(new List<object>(), "HomestayID", "Name");
                return View(new Room()); // Trả về room rỗng để tránh lỗi
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateAjax(IFormCollection form, IFormFile ImageFile)
        {
            try
            {
                int roomId = int.Parse(form["RoomID"]);
                var room = await _roomRepo.GetByIdAsync(roomId);
                if (room == null)
                    return Json(new { success = false, error = "Không tìm thấy phòng." });

                room.RoomName = form["RoomName"];
                room.PricePerNight = decimal.Parse(form["PricePerNight"]);
                room.Status = form["Status"];
                room.HomestayID = int.Parse(form["HomestayID"]);

                if (ImageFile != null && ImageFile.Length > 0)
                {
                    var fileName = Path.GetFileName(ImageFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/rooms");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await ImageFile.CopyToAsync(stream);
                    }
                    room.ImageUrl = "/images/rooms/" + fileName;
                }

                await _roomRepo.UpdateAsync(room);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAjax(int id)
        {
            try
            {
                var exists = await _roomRepo.ExistsAsync(id);
                if (!exists)
                    return Json(new { success = false, error = "Phòng không tồn tại." });

                await _roomRepo.DeleteAsync(id);
                return Json(new { success = true });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }
    }
}