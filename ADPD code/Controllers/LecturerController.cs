using ADPD_code.Data;
using ADPD_code.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADPD_code.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context; 

        public LecturerController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }
            return View();
        }

        // Action hiển thị profile giảng viên
        public async Task<IActionResult> Profile()
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy LecturerID từ Session
            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            if (lecturerId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Truy vấn thông tin giảng viên với các thông tin liên quan
            var lecturer = await _context.Lecturers
                .Include(l => l.Department)
                .Include(l => l.Courses)
                    .ThenInclude(c => c.Enrollments)
                .Include(l => l.Assignments)
                    .ThenInclude(a => a.Course)
                .Include(l => l.Assignments)
                    .ThenInclude(a => a.AssignmentSubmissions)
                .FirstOrDefaultAsync(l => l.LecturerID == lecturerId);

            if (lecturer == null)
            {
                return NotFound();
            }

            return View(lecturer);
        }

        // Action chỉnh sửa thông tin giảng viên
        [HttpGet]
        public async Task<IActionResult> Edit(int? id)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            if (lecturerId == null || (id != null && id != lecturerId))
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturer = await _context.Lecturers
                .Include(l => l.Department)
                .FirstOrDefaultAsync(l => l.LecturerID == lecturerId);

            if (lecturer == null)
            {
                return NotFound();
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(lecturer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Lecturer lecturer)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            if (id != lecturerId || id != lecturer.LecturerID)
            {
                return RedirectToAction("Index", "Login");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Chỉ cập nhật các trường được phép
                    var existingLecturer = await _context.Lecturers.FindAsync(id);
                    if (existingLecturer != null)
                    {
                        existingLecturer.FullName = lecturer.FullName;
                        existingLecturer.Email = lecturer.Email;
                        existingLecturer.Phone = lecturer.Phone;
                        existingLecturer.DepartmentID = lecturer.DepartmentID;

                        await _context.SaveChangesAsync();
                        TempData["SuccessMessage"] = "Cập nhật thông tin thành công!";
                        return RedirectToAction(nameof(Profile));
                    }
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!LecturerExists(lecturer.LecturerID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            ViewBag.Departments = await _context.Departments.ToListAsync();
            return View(lecturer);
        }

        // Action xem danh sách môn học
        public async Task<IActionResult> Courses()
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            if (lecturerId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var courses = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Where(c => c.LecturerID == lecturerId)
                .ToListAsync();

            return View(courses);
        }

        // Action xem chi tiết môn học
        public async Task<IActionResult> CourseDetail(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            var course = await _context.Courses
                .Include(c => c.Enrollments)
                    .ThenInclude(e => e.Student)
                .Include(c => c.Lecturer)
                .FirstOrDefaultAsync(c => c.CourseID == id && c.LecturerID == lecturerId);

            if (course == null)
            {
                return NotFound();
            }

            return View(course);
        }

        // Action quản lý bài tập
        public async Task<IActionResult> Assignments()
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            if (lecturerId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.AssignmentSubmissions)
                .Where(a => a.LecturerID == lecturerId)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            return View(assignments);
        }

        // Action tạo bài tập mới
        [HttpGet]
        public async Task<IActionResult> CreateAssignment()
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            if (lecturerId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách môn học của giảng viên
            ViewBag.Courses = await _context.Courses
                .Where(c => c.LecturerID == lecturerId)
                .ToListAsync();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateAssignment(Assignment assignment)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            if (lecturerId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            if (ModelState.IsValid)
            {
                assignment.LecturerID = lecturerId.Value;
                _context.Add(assignment);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Tạo bài tập thành công!";
                return RedirectToAction(nameof(Assignments));
            }

            ViewBag.Courses = await _context.Courses
                .Where(c => c.LecturerID == lecturerId)
                .ToListAsync();

            return View(assignment);
        }

        // Action xem chi tiết bài tập và các bài nộp
        public async Task<IActionResult> AssignmentDetail(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.AssignmentSubmissions)
                    .ThenInclude(s => s.Student)
                .Include(a => a.AssignmentAttachments)
                .FirstOrDefaultAsync(a => a.AssignmentID == id && a.LecturerID == lecturerId);

            if (assignment == null)
            {
                return NotFound();
            }

            return View(assignment);
        }

        // Action chấm điểm bài nộp
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GradeSubmission(int submissionId, float score, string feedback)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");

            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.SubmissionID == submissionId && s.Assignment != null && s.Assignment.LecturerID == lecturerId);

            if (submission == null)
            {
                return NotFound();
            }

            submission.Score = score;
            submission.Feedback = feedback;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Chấm điểm thành công!";
            return RedirectToAction(nameof(AssignmentDetail), new { id = submission.AssignmentID });
        }

        // Helper method
        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(e => e.LecturerID == id);
        }
    }
}