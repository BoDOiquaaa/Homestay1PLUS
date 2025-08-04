using Homestay1.Repositories;
using Homestay1.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace Homestay1.Areas.ad.Controllers
{
    [Area("ad")]
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepo;

        public AccountController(IAccountRepository accountRepo)
        {
            _accountRepo = accountRepo;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Kiểm tra nếu đã đăng nhập
            var userID = HttpContext.Session.GetInt32("UserID");
            var roleID = HttpContext.Session.GetInt32("RoleID");

            if (userID.HasValue && roleID.HasValue)
            {
                // Nếu đã đăng nhập và là Owner -> redirect đến Users
                if (roleID.Value == 2)
                {
                    return RedirectToAction("Index", "Users", new { area = "ad" });
                }
                // Nếu đã đăng nhập nhưng không phải Owner -> về trang chủ
                else
                {
                    return RedirectToAction("Index", "Home", new { area = "" });
                }
            }

            // Chưa đăng nhập -> trả về view chứa form AJAX
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<JsonResult> LoginAjax([FromBody] LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                                              .Select(e => e.ErrorMessage)
                                              .ToList();
                return Json(new { success = false, errors });
            }

            var user = await _accountRepo.AuthenticateAsync(vm.Email, vm.Password);
            if (user == null)
            {
                return Json(new { success = false, errors = new[] { "Email hoặc mật khẩu không đúng." } });
            }

            // Lưu session với thông tin đầy đủ
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetInt32("RoleID", user.RoleID);
            HttpContext.Session.SetString("UserName", user.FullName ?? "");
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");

            // Debug log
            System.Diagnostics.Debug.WriteLine($"=== USER LOGIN SUCCESS ===");
            System.Diagnostics.Debug.WriteLine($"UserID: {user.UserID}");
            System.Diagnostics.Debug.WriteLine($"RoleID: {user.RoleID}");
            System.Diagnostics.Debug.WriteLine($"FullName: {user.FullName}");
            System.Diagnostics.Debug.WriteLine($"Email: {user.Email}");

            // Kiểm tra quyền và redirect phù hợp
            if (user.RoleID == 2) // Owner
            {
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Index", "Users", new { area = "ad" }),
                    message = $"Chào mừng Owner {user.FullName}!"
                });
            }
            else if (user.RoleID == 1) // Admin - cũng cho phép truy cập
            {
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Index", "Users", new { area = "ad" }),
                    message = $"Chào mừng Admin {user.FullName}!"
                });
            }
            else
            {
                // Staff hoặc Customer -> về trang chủ
                return Json(new
                {
                    success = true,
                    redirectUrl = Url.Action("Index", "Home", new { area = "" }),
                    message = $"Chào {user.FullName}! Bạn không có quyền quản lý."
                });
            }
        }

        public IActionResult Logout()
        {
            // Clear toàn bộ session
            HttpContext.Session.Clear();

            // Debug log
            System.Diagnostics.Debug.WriteLine("=== USER LOGOUT ===");
            System.Diagnostics.Debug.WriteLine("Session cleared");

            TempData["Info"] = "Bạn đã đăng xuất thành công!";
            return RedirectToAction("Login");
        }

        // Action để kiểm tra thông tin session hiện tại (for debugging)
        public IActionResult SessionInfo()
        {
            var sessionData = new
            {
                UserID = HttpContext.Session.GetInt32("UserID"),
                RoleID = HttpContext.Session.GetInt32("RoleID"),
                UserName = HttpContext.Session.GetString("UserName"),
                UserEmail = HttpContext.Session.GetString("UserEmail"),
                SessionId = HttpContext.Session.Id
            };

            return Json(sessionData);
        }
    }
}