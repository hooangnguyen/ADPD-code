using ADPD_code.Data;
using ADPD_code.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADPD_code.Controllers
{
    public class RegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Hiển thị form đăng ký
        [HttpGet]
        public IActionResult Index()
        {
            return View();
        }

        // POST: Xử lý đăng ký sinh viên
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(
            string studentId,
            string firstName,
            string lastName,
            string email,
            DateTime birthDate,
            string gender,
            string phone,
            string password,
            string confirmPassword)
        {
            try
            {
                // 1. Validation cơ bản
                if (string.IsNullOrEmpty(studentId) || string.IsNullOrEmpty(firstName) ||
                    string.IsNullOrEmpty(lastName) || string.IsNullOrEmpty(email) ||
                    string.IsNullOrEmpty(password) || string.IsNullOrEmpty(confirmPassword))
                {
                    ModelState.AddModelError("", "Vui lòng điền đầy đủ thông tin bắt buộc.");
                    return View("Index");
                }

                // 2. Kiểm tra mật khẩu khớp
                if (password != confirmPassword)
                {
                    ModelState.AddModelError("", "Mật khẩu xác nhận không khớp.");
                    return View("Index");
                }

                // 3. Kiểm tra độ dài mật khẩu
                if (password.Length < 6)
                {
                    ModelState.AddModelError("", "Mật khẩu phải có ít nhất 6 ký tự.");
                    return View("Index");
                }

                // 4. Kiểm tra email đã tồn tại
                var existingEmail = await _context.Students
                    .AnyAsync(s => s.Email == email);
                if (existingEmail)
                {
                    ModelState.AddModelError("", "Email này đã được đăng ký.");
                    return View("Index");
                }

                // 5. Kiểm tra username (studentId) đã tồn tại trong Account
                var existingUsername = await _context.Accounts
                    .AnyAsync(a => a.Username == studentId);
                if (existingUsername)
                {
                    ModelState.AddModelError("", "Mã sinh viên này đã được đăng ký.");
                    return View("Index");
                }

                // 6. Tạo Student mới
                var newStudent = new Student
                {
                    FullName = $"{firstName} {lastName}".Trim(),
                    Gender = gender == "male" ? "Nam" : "Nữ",
                    DOB = birthDate,
                    Email = email,
                    Phone = phone,
                    Address = "", // Có thể thêm field Address vào form nếu cần
                    Status = "Đang học"
                };

                _context.Students.Add(newStudent);
                await _context.SaveChangesAsync();

                // 7. Tạo Account cho sinh viên
                // ✅ Hash password bằng BCrypt
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(password);

                var newAccount = new Account
                {
                    Username = studentId, // Dùng mã sinh viên làm username
                    PasswordHash = hashedPassword, // Password đã được hash
                    Role = "Student",
                    StudentID = newStudent.StudentId,
                    LecturerID = null
                };

                _context.Accounts.Add(newAccount);
                await _context.SaveChangesAsync();

                // 8. Đăng ký thành công - Chuyển đến trang đăng nhập
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Index", "Login");
            }
            catch (Exception ex)
            {
                // Log lỗi (có thể dùng ILogger)
                System.Diagnostics.Debug.WriteLine($"Register Error: {ex.Message}");
                ModelState.AddModelError("", "Đã xảy ra lỗi khi đăng ký. Vui lòng thử lại.");
                return View("Index");
            }
        }
    }
}