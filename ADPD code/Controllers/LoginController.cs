using ADPD_code.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ADPD_code.Controllers
{
    // Controller này chỉ xử lý các tác vụ liên quan đến Đăng nhập/Đăng xuất.
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
                var roleLower = role.ToLowerInvariant();
                if (roleLower == "student")
                    return Redirect("/Student/Dashboard");
                if (roleLower == "lecturer")
                    return Redirect("/Lecturer/Dashboard");
                if (roleLower == "admin")
                    return Redirect("/Admin/Dashboard");

                // Default fallback
                return RedirectToAction("Dashboard", "Home");
            }
            return View(); // Trả về View Index.cshtml (chứa form login)
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        // Sử dụng tham số trực tiếp (như yêu cầu)
        public async Task<IActionResult> Login(string username, string password)
        {
            // 1. Kiểm tra rỗng (Validation cơ bản, nếu dùng ViewModel sẽ tốt hơn)
            if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password))
            {
                ModelState.AddModelError(string.Empty, "Vui lòng nhập đầy đủ Tên đăng nhập và Mật khẩu.");
                return View("Index");
            }
            
            // 2. Tìm kiếm tài khoản
            var account = await _context.Accounts
                                        // ⚠️ LƯU Ý BẢO MẬT: Mật khẩu phải được băm (hashing) trước khi so sánh
                                        // Chúng ta đang so sánh mật khẩu thô (GIẢ ĐỊNH PasswordHash đang chứa mật khẩu thô)
                                        .FirstOrDefaultAsync(u => u.Username == username);

            // Tách tìm kiếm và kiểm tra password để xử lý hashing sau này
            if (account == null)
            {
                ModelState.AddModelError(string.Empty, "Tên đăng nhập không tồn tại.");
                return View("Index");
            }

            // ❌ DÒNG NÀY PHẢI ĐƯỢC THAY BẰNG MỘT HÀM KIỂM TRA MẬT KHẨU BĂM (e.g., VerifyPassword(password, account.PasswordHash))
            if (account.PasswordHash != password) 
            {
                ModelState.AddModelError(string.Empty, "Mật khẩu không chính xác.");
                return View("Index");
            }
            
            // 3. Đăng nhập thành công: Thiết lập Session
                HttpContext.Session.SetString("Username", account.Username ?? string.Empty);
            HttpContext.Session.SetInt32("UserID", account.UserID); // Dùng UserID
            HttpContext.Session.SetString("Role", account.Role ?? string.Empty); // Lấy Role để phân quyền

            if (account.StudentID.HasValue)
                HttpContext.Session.SetInt32("StudentID", account.StudentID.Value);
            if (account.LecturerID.HasValue)
                HttpContext.Session.SetInt32("LecturerID", account.LecturerID.Value);

            // 4. Điều hướng sau phân quyền
            var roleNormalized = (account.Role ?? string.Empty).ToLowerInvariant();
            if (roleNormalized == "student")
            {
                // Redirect to student dashboard (adjust path if your route differs)
                return Redirect("/Student/Dashboard");
            }
            else if (roleNormalized == "lecturer")
            {
                return Redirect("/Lecturer/Dashboard");
            }
            else if (roleNormalized == "admin")
            {
                return Redirect("/Admin/Dashboard");
            }
            else
            {
                // Default fallback
                return RedirectToAction("Dashboard", "Home");
            }
        }
        // 3. LOGOUT: Đăng xuất
        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear(); // Xóa toàn bộ Session
            return RedirectToAction("Index", "Home"); // Quay về trang chủ
        }
    }
}