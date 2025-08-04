using Homestay1.Models;
using Homestay1.Repositories;
using Homestay1.Repository;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;

namespace Homestay1.Controllers
{
    public class HomeController : Controller
    {
        private readonly IHomeRepository _homeRepo;
        private readonly IHomestayRepository _homestayRepo;

        // ← SỬA CONSTRUCTOR: Inject cả 2 repository
        public HomeController(IHomeRepository homeRepo, IHomestayRepository homestayRepo)
        {
            _homeRepo = homeRepo;
            _homestayRepo = homestayRepo;  // ← THÊM DÒNG NÀY
        }

        public async Task<IActionResult> Index()
        {
            var homestays = await _homeRepo.GetAllHomestaysAsync();
            return View(homestays);
        }

        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var homestay = await _homestayRepo.GetByIdWithRoomsAsync(id);
                if (homestay == null)
                {
                    return NotFound();
                }
                return View(homestay);
            }
            catch (Exception ex)
            {
                // Log error for debugging
                ViewBag.Error = ex.Message;
                return View("Error");
            }
        }
    }
}