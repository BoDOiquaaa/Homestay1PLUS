using Homestay1.Data;
using Homestay1.Models.Entities;
using Homestay1.Repositories;
using Homestay1.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Homestay1.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAccountRepository _accountRepo;
        private readonly ApplicationDbContext _db;

        public AccountController(IAccountRepository accountRepo, ApplicationDbContext db)
        {
            _accountRepo = accountRepo;
            _db = db;
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Kiểm tra nếu đã đăng nhập
            var userID = HttpContext.Session.GetInt32("UserID");
            if (userID.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            var user = await _accountRepo.AuthenticateAsync(vm.Email, vm.Password);
            if (user == null)
            {
                ModelState.AddModelError("", "Email hoặc mật khẩu không đúng.");
                return View(vm);
            }

            // Lưu session
            HttpContext.Session.SetInt32("UserID", user.UserID);
            HttpContext.Session.SetInt32("RoleID", user.RoleID);
            HttpContext.Session.SetString("UserName", user.FullName ?? "");
            HttpContext.Session.SetString("UserEmail", user.Email ?? "");

            TempData["Success"] = $"Chào mừng {user.FullName}!";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Kiểm tra nếu đã đăng nhập
            var userID = HttpContext.Session.GetInt32("UserID");
            if (userID.HasValue)
            {
                return RedirectToAction("Index", "Home");
            }

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return View(vm);
                }

                // Kiểm tra email đã tồn tại
                if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
                {
                    ModelState.AddModelError("Email", "Email này đã được sử dụng.");
                    return View(vm);
                }

                // Tạo user mới với RoleID = 4 (Customer)
                var user = new User
                {
                    RoleID = 4, // Customer role
                    FullName = vm.FullName?.Trim(),
                    Email = vm.Email?.Trim().ToLower(),
                    Password = vm.Password, // Trong thực tế nên hash password
                    Phone = vm.Phone?.Trim(),
                    CreatedAt = DateTime.Now
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                // Tự động đăng nhập sau khi đăng ký
                HttpContext.Session.SetInt32("UserID", user.UserID);
                HttpContext.Session.SetInt32("RoleID", user.RoleID);
                HttpContext.Session.SetString("UserName", user.FullName ?? "");
                HttpContext.Session.SetString("UserEmail", user.Email ?? "");

                TempData["Success"] = "Đăng ký thành công! Chào mừng bạn đến với Luxury Resort!";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Register Error: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra khi đăng ký. Vui lòng thử lại.");
                return View(vm);
            }
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["Info"] = "Bạn đã đăng xuất thành công!";
            return RedirectToAction("Index", "Home");
        }

        // Helper method để validate email
        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
    }
}