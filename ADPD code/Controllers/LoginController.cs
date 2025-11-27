using ADPD_code.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ADPD_code.Controllers
{
    public class LoginController : Controller
    {
        private readonly ApplicationDbContext _context;

        public LoginController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. [GET] Index: Hiển thị form đăng nhập
        [HttpGet]
        public IActionResult Index()
        {
            // Kiểm tra Session: Nếu đã có Role trong Session, người dùng đã đăng nhập.
            var role = HttpContext.Session.GetString("Role");
            if (!string.IsNullOrEmpty(role))
            {
                // Chuyển hướng theo role nếu đã đăng nhập
                return RedirectToDashboard(role);
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string username, string password)
        {
            // 1. Kiểm tra rỗng
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu.");
                return View("Index");
            }

            // 2. Tìm kiếm tài khoản
            var account = await _context.Accounts
                .Include(a => a.Student)
                .Include(a => a.Lecturer)
                .FirstOrDefaultAsync(u => u.Username == username);

            if (account == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập không tồn tại.");
                return View("Index");
            }

            // 3. ✅ Kiểm tra mật khẩu bằng BCrypt
            bool isPasswordValid = false;

            try
            {
                // Thử verify với BCrypt
                isPasswordValid = BCrypt.Net.BCrypt.Verify(password, account.PasswordHash);
            }
            catch
            {
                // Nếu lỗi (password chưa hash), so sánh trực tiếp (backward compatibility)
                // CHỈ để test, XÓA sau khi đã hash hết password trong DB
                isPasswordValid = (account.PasswordHash == password);
            }

            if (!isPasswordValid)
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
                return View("Index");
            }

            // 4. Đăng nhập thành công: Thiết lập Session
            HttpContext.Session.SetString("Username", account.Username ?? string.Empty);
            HttpContext.Session.SetInt32("UserID", account.UserID);
            HttpContext.Session.SetString("Role", account.Role ?? string.Empty);

            // Lấy tên đầy đủ tùy theo role
            string fullName = "User";
            if (account.StudentID.HasValue && account.Student != null)
            {
                HttpContext.Session.SetInt32("StudentID", account.StudentID.Value);
                fullName = account.Student.FullName ?? username;
            }
            else if (account.LecturerID.HasValue && account.Lecturer != null)
            {
                HttpContext.Session.SetInt32("LecturerID", account.LecturerID.Value);
                fullName = account.Lecturer.FullName ?? username;
            }
            else if (account.Role == "Admin")
            {
                fullName = "Administrator";
            }

            HttpContext.Session.SetString("FullName", fullName);

            // 5. Điều hướng sau phân quyền
            return RedirectToDashboard(account.Role);
        }

        // 3. LOGOUT: Đăng xuất
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Đã đăng xuất thành công!";
            return RedirectToAction("Index", "Login");
        }

        // Helper method để redirect theo role
        private IActionResult RedirectToDashboard(string role)
        {
            var roleNormalized = (role ?? string.Empty).ToLowerInvariant();

            return roleNormalized switch
            {
                "student" => RedirectToAction("Dashboard", "Student"),
                "lecturer" => RedirectToAction("Dashboard", "Lecturer"),
                "admin" => RedirectToAction("Dashboard", "Admin"),
                _ => RedirectToAction("Index", "Home")
            };
        }
    }
}