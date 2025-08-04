using Homestay1.Data;
using Homestay1.Models.Entities;
using Homestay1.Repositories;
using Homestay1.ViewModels;
using Homestay1.Filters; // ← THÊM USING CHO FILTER
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace Homestay1.Areas.ad.Controllers
{
    [Area("ad")]
    [OwnerAuthorization] // ← THÊM FILTER ĐỂ KIỂM TRA QUYỀN
    public class UsersController : Controller
    {
        private readonly IUserRepository _repo;
        private readonly ApplicationDbContext _db;

        public UsersController(IUserRepository repo, ApplicationDbContext db)
        {
            _repo = repo;
            _db = db;
        }

        /// <summary>
        /// Đổ danh sách Roles từ DB vào ViewBag.Roles
        /// </summary>
        private async Task PopulateRolesAsync(int? selectedRoleID = null)
        {
            try
            {
                var allRoles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();

                System.Diagnostics.Debug.WriteLine($"🔎 PopulateRoles - Found {allRoles.Count} roles");
                System.Diagnostics.Debug.WriteLine($"🔎 Selected RoleID: {selectedRoleID}");

                foreach (var role in allRoles)
                {
                    System.Diagnostics.Debug.WriteLine($"Role: ID={role.RoleID}, Name={role.RoleName}");
                }

                // Tạo SelectList với selectedValue
                if (selectedRoleID.HasValue)
                {
                    ViewBag.Roles = new SelectList(allRoles, "RoleID", "RoleName", selectedRoleID.Value);
                    System.Diagnostics.Debug.WriteLine($"✅ ViewBag.Roles created with selected value: {selectedRoleID.Value}");
                }
                else
                {
                    ViewBag.Roles = new SelectList(allRoles, "RoleID", "RoleName");
                    System.Diagnostics.Debug.WriteLine("✅ ViewBag.Roles created without selected value");
                }

                // Debug: kiểm tra SelectList
                var selectList = (SelectList)ViewBag.Roles;
                System.Diagnostics.Debug.WriteLine($"SelectList SelectedValue: {selectList.SelectedValue}");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in PopulateRolesAsync: {ex.Message}");
                ViewBag.Roles = new SelectList(new List<Role>(), "RoleID", "RoleName");
            }
        }

        // GET: /ad/Users/Create
        public async Task<IActionResult> Create()
        {
            try
            {
                // Lấy toàn bộ Roles từ database và gán vào ViewBag
                ViewBag.Roles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();

                // Debug log
                var roles = (List<Role>)ViewBag.Roles;
                System.Diagnostics.Debug.WriteLine($"=== ROLES DEBUG ===");
                System.Diagnostics.Debug.WriteLine($"Total roles: {roles.Count}");
                foreach (var role in roles)
                {
                    System.Diagnostics.Debug.WriteLine($"Role: ID={role.RoleID}, Name={role.RoleName}");
                }

                return View(new UserViewModel());
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in Create GET: {ex.Message}");
                ViewBag.Roles = new List<Role>(); // Empty list fallback
                return View(new UserViewModel());
            }
        }

        // POST: /ad/Users/Create
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(UserViewModel vm)
        {
            try
            {
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"=== CREATE USER REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"Current User - UserID: {HttpContext.Session.GetInt32("UserID")}, RoleID: {HttpContext.Session.GetInt32("RoleID")}");
                System.Diagnostics.Debug.WriteLine($"RoleID: {vm.RoleID}");
                System.Diagnostics.Debug.WriteLine($"FullName: '{vm.FullName}'");
                System.Diagnostics.Debug.WriteLine($"Email: '{vm.Email}'");
                System.Diagnostics.Debug.WriteLine($"Password: {(string.IsNullOrEmpty(vm.Password) ? "EMPTY" : $"PROVIDED ({vm.Password.Length} chars)")}");
                System.Diagnostics.Debug.WriteLine($"Phone: '{vm.Phone}'");
                System.Diagnostics.Debug.WriteLine($"ModelState.IsValid: {ModelState.IsValid}");

                // CLEAR ModelState để tránh lỗi validation không cần thiết
                ModelState.Clear();

                // 1) Manual validation - ƯU TIÊN VALIDATE THỦ CÔNG
                var errors = new List<string>();

                if (vm.RoleID <= 0)
                {
                    errors.Add("Vui lòng chọn vai trò");
                    ModelState.AddModelError("RoleID", "Vui lòng chọn vai trò");
                }

                if (string.IsNullOrWhiteSpace(vm.FullName))
                {
                    errors.Add("Vui lòng nhập họ và tên");
                    ModelState.AddModelError("FullName", "Vui lòng nhập họ và tên");
                }

                if (string.IsNullOrWhiteSpace(vm.Email))
                {
                    errors.Add("Vui lòng nhập email");
                    ModelState.AddModelError("Email", "Vui lòng nhập email");
                }

                if (string.IsNullOrWhiteSpace(vm.Password))
                {
                    errors.Add("Vui lòng nhập mật khẩu");
                    ModelState.AddModelError("Password", "Vui lòng nhập mật khẩu");
                }

                // Nếu có lỗi manual validation
                if (errors.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Manual validation failed: {string.Join(", ", errors)}");

                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        Response.StatusCode = 400;
                        return Json(new { success = false, errors = errors });
                    }

                    ViewBag.Roles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();
                    return View(vm);
                }

                // 2) Validate email format
                if (!IsValidEmail(vm.Email))
                {
                    var emailError = "Email không hợp lệ";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        Response.StatusCode = 400;
                        return Json(new { success = false, errors = new[] { emailError } });
                    }

                    ModelState.AddModelError("Email", emailError);
                    ViewBag.Roles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();
                    return View(vm);
                }

                // 3) Check email duplicate
                if (await _db.Users.AnyAsync(u => u.Email == vm.Email))
                {
                    var duplicateError = "Email này đã tồn tại";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        Response.StatusCode = 400;
                        return Json(new { success = false, errors = new[] { duplicateError } });
                    }

                    ModelState.AddModelError("Email", duplicateError);
                    ViewBag.Roles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();
                    return View(vm);
                }

                // 4) Bảo đảm Role hợp lệ
                if (!await _db.Roles.AnyAsync(r => r.RoleID == vm.RoleID))
                {
                    var roleError = "Vai trò không tồn tại";
                    if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        Response.StatusCode = 400;
                        return Json(new { success = false, errors = new[] { roleError } });
                    }

                    ModelState.AddModelError("RoleID", roleError);
                    ViewBag.Roles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();
                    return View(vm);
                }

                // 5) Build entity và save
                var user = new User
                {
                    RoleID = vm.RoleID,
                    FullName = vm.FullName?.Trim(),
                    Email = vm.Email?.Trim().ToLower(), // Normalize email
                    Password = vm.Password, // Trong thực tế nên hash password
                    Phone = vm.Phone?.Trim(),
                    CreatedAt = DateTime.Now
                };

                _db.Users.Add(user);
                await _db.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine("✅ User created successfully");

                // 6) Nếu AJAX -> trả về JSON để client tự redirect
                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    var url = Url.Action(nameof(Index), "Users", new { area = "ad" });
                    return Json(new { success = true, redirectUrl = url });
                }

                // 7) Nếu không phải AJAX -> redirect server-side
                TempData["Success"] = "Thêm user thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                // Log chi tiết lỗi
                System.Diagnostics.Debug.WriteLine($"❌ ERROR: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ STACK TRACE: {ex.StackTrace}");

                if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                {
                    Response.StatusCode = 500;
                    return Json(new
                    {
                        success = false,
                        message = "Có lỗi xảy ra khi tạo user: " + ex.Message,
                        errors = new[] { ex.Message }
                    });
                }

                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                ViewBag.Roles = await _db.Roles.OrderBy(r => r.RoleName).ToListAsync();
                return View(vm);
            }
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

        // GET: /ad/Users
        public async Task<IActionResult> Index(string search)
        {
            ViewBag.Search = search;
            var list = await _repo.GetAllAsync(search);
            return View(list);
        }

        // GET: /ad/Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== EDIT GET REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"UserID: {id}");

                if (id <= 0)
                {
                    System.Diagnostics.Debug.WriteLine("❌ Invalid UserID");
                    return BadRequest("ID không hợp lệ");
                }

                var u = await _repo.GetByIdAsync(id);

                if (u == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ User not found with ID: {id}");
                    return NotFound($"Không tìm thấy user với ID: {id}");
                }

                System.Diagnostics.Debug.WriteLine($"✅ User found: {u.FullName} - {u.Email}");
                System.Diagnostics.Debug.WriteLine($"User RoleID: {u.RoleID}");

                // Populate roles với selected value
                await PopulateRolesAsync(u.RoleID);

                // Create ViewModel
                var vm = new UserViewModel
                {
                    UserID = u.UserID,
                    RoleID = u.RoleID,
                    FullName = u.FullName,
                    Email = u.Email,
                    Phone = u.Phone
                };

                System.Diagnostics.Debug.WriteLine($"ViewModel created - UserID: {vm.UserID}, RoleID: {vm.RoleID}");

                return View(vm);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in Edit GET: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ STACK TRACE: {ex.StackTrace}");

                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin user: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: /ad/Users/Edit
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UserViewModel vm)
        {
            try
            {
                // Debug logging
                System.Diagnostics.Debug.WriteLine($"=== EDIT USER REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"UserID: {vm.UserID}");
                System.Diagnostics.Debug.WriteLine($"RoleID: {vm.RoleID}");
                System.Diagnostics.Debug.WriteLine($"FullName: '{vm.FullName}'");
                System.Diagnostics.Debug.WriteLine($"Email: '{vm.Email}'");
                System.Diagnostics.Debug.WriteLine($"Phone: '{vm.Phone}'");

                // Clear ModelState để tránh validation cũ
                ModelState.Clear();

                // Manual validation
                var errors = new List<string>();

                if (!vm.UserID.HasValue || vm.UserID <= 0)
                {
                    errors.Add("UserID không hợp lệ");
                    ModelState.AddModelError("UserID", "UserID không hợp lệ");
                }

                if (vm.RoleID <= 0)
                {
                    errors.Add("Vui lòng chọn vai trò");
                    ModelState.AddModelError("RoleID", "Vui lòng chọn vai trò");
                }

                if (string.IsNullOrWhiteSpace(vm.FullName))
                {
                    errors.Add("Vui lòng nhập họ và tên");
                    ModelState.AddModelError("FullName", "Vui lòng nhập họ và tên");
                }

                if (string.IsNullOrWhiteSpace(vm.Email))
                {
                    errors.Add("Vui lòng nhập email");
                    ModelState.AddModelError("Email", "Vui lòng nhập email");
                }

                // Nếu có lỗi validation
                if (errors.Any())
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Validation failed: {string.Join(", ", errors)}");
                    await PopulateRolesAsync();
                    return View(vm);
                }

                // Validate email format
                if (!IsValidEmail(vm.Email))
                {
                    ModelState.AddModelError("Email", "Email không hợp lệ");
                    await PopulateRolesAsync();
                    return View(vm);
                }

                // Tìm user cần update
                var u = await _repo.GetByIdAsync(vm.UserID.Value);
                if (u == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ User not found with ID: {vm.UserID}");
                    return NotFound();
                }

                // Check email duplicate (trừ chính user hiện tại)
                if (await _db.Users.AnyAsync(user => user.Email == vm.Email && user.UserID != vm.UserID))
                {
                    ModelState.AddModelError("Email", "Email này đã tồn tại");
                    await PopulateRolesAsync();
                    return View(vm);
                }

                // Check role tồn tại
                if (!await _db.Roles.AnyAsync(r => r.RoleID == vm.RoleID))
                {
                    ModelState.AddModelError("RoleID", "Vai trò không tồn tại");
                    await PopulateRolesAsync();
                    return View(vm);
                }

                // Update thông tin user
                u.RoleID = vm.RoleID;
                u.FullName = vm.FullName?.Trim();
                u.Email = vm.Email?.Trim().ToLower();
                u.Phone = vm.Phone?.Trim();

                System.Diagnostics.Debug.WriteLine($"Updating user: ID={u.UserID}, Name={u.FullName}, Email={u.Email}");

                // Sử dụng _db.SaveChangesAsync() thay vì _repo.UpdateAsync()
                _db.Users.Update(u);
                var result = await _db.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Update result: {result} rows affected");

                if (result > 0)
                {
                    TempData["Success"] = "Cập nhật user thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("❌ No rows affected during update");
                    ModelState.AddModelError("", "Không thể cập nhật user");
                    await PopulateRolesAsync();
                    return View(vm);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in Edit: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"❌ STACK TRACE: {ex.StackTrace}");

                ModelState.AddModelError("", "Có lỗi xảy ra khi cập nhật: " + ex.Message);
                await PopulateRolesAsync();
                return View(vm);
            }
        }

        // POST: /ad/Users/Delete
        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"=== DELETE USER REQUEST ===");
                System.Diagnostics.Debug.WriteLine($"UserID to delete: {id}");
                System.Diagnostics.Debug.WriteLine($"Current User - UserID: {HttpContext.Session.GetInt32("UserID")}, RoleID: {HttpContext.Session.GetInt32("RoleID")}");

                await _repo.DeleteAsync(id);

                System.Diagnostics.Debug.WriteLine("✅ User deleted successfully");
                TempData["Success"] = "Xóa user thành công!";

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERROR in Delete: {ex.Message}");
                TempData["Error"] = "Có lỗi xảy ra khi xóa user: " + ex.Message;
                return RedirectToAction(nameof(Index));
            }
        }
    }
}