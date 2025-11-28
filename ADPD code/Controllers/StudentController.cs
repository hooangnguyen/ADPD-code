using ADPD_code.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADPD_code.Controllers
{
    public class StudentController : Controller
    {
        private readonly ApplicationDbContext _context;

        public StudentController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard - Trang chủ sinh viên
        public async Task<IActionResult> Dashboard(bool partial = false)
        {
            // Kiểm tra đăng nhập và role
            if (HttpContext.Session.GetString("Role") != "Student")
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy thông tin sinh viên từ Session
            var username = HttpContext.Session.GetString("Username");
            var studentId = HttpContext.Session.GetInt32("StudentID");

            if (studentId.HasValue)
            {
                // Lấy thông tin chi tiết sinh viên từ DB
                var student = await _context.Students
                    .FirstOrDefaultAsync(s => s.StudentId == studentId.Value);

                if (student != null)
                {
                    ViewBag.FullName = student.FullName;
                    ViewBag.StudentId = studentId.Value;
                    ViewBag.Email = student.Email;
                    ViewBag.Phone = student.Phone;
                }
            }

            ViewBag.Username = username;

            if (partial)
            {
                return PartialView("_DashboardHomePartial");
            }

            return View();
        }

        // Profile - Hồ sơ sinh viên
        public async Task<IActionResult> Profile(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Student")
            {
                return RedirectToAction("Index", "Login");
            }

            var studentId = HttpContext.Session.GetInt32("StudentID");
            if (!studentId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var student = await _context.Students
                .Include(s => s.StudentClasses)
                .ThenInclude(sc => sc.Class)
                .ThenInclude(c => c.Major)
                .FirstOrDefaultAsync(s => s.StudentId == studentId.Value);

            if (student == null)
            {
                return NotFound();
            }

            ViewData["IsPartial"] = partial;
            return partial ? PartialView(student) : View(student);
        }

        // Grades - Xem điểm
        public Task<IActionResult> Grades(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Student")
            {
                return Task.FromResult<IActionResult>(RedirectToAction("Index", "Login"));
            }

            var studentId = HttpContext.Session.GetInt32("StudentID");
            if (!studentId.HasValue)
            {
                return Task.FromResult<IActionResult>(RedirectToAction("Index", "Login"));
            }

            // TODO: Lấy danh sách điểm từ bảng Enrollment
            ViewBag.StudentId = studentId.Value;
            ViewData["IsPartial"] = partial;
            return Task.FromResult<IActionResult>(partial ? PartialView() : View());
        }

        // Schedule - Lịch học
        public IActionResult Schedule(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Student")
            {
                return RedirectToAction("Index", "Login");
            }

            var studentId = HttpContext.Session.GetInt32("StudentID");
            if (!studentId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // TODO: Lấy lịch học từ DB
            ViewBag.StudentId = studentId.Value;
            ViewData["IsPartial"] = partial;
            return partial ? PartialView() : View();
        }

        // Assignments - Bài tập
        public Task<IActionResult> Assignments(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Student")
            {
                return Task.FromResult<IActionResult>(RedirectToAction("Index", "Login"));
            }

            var studentId = HttpContext.Session.GetInt32("StudentID");
            if (!studentId.HasValue)
            {
                return Task.FromResult<IActionResult>(RedirectToAction("Index", "Login"));
            }

            // TODO: Lấy danh sách bài tập
            ViewBag.StudentId = studentId.Value;
            ViewData["IsPartial"] = partial;
            return Task.FromResult<IActionResult>(partial ? PartialView() : View());
        }
    }
}