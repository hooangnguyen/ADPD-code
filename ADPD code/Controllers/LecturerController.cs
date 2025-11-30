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

        public async Task<IActionResult> Dashboard(bool partial = false)
        {
            var role = HttpContext.Session.GetString("Role");
            if (string.IsNullOrEmpty(role) || !role.Equals("Lecturer", StringComparison.OrdinalIgnoreCase))
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            if (lecturerId == null)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy thông tin giảng viên
            var lecturer = await _context.Lecturers
                .FirstOrDefaultAsync(l => l.LecturerID == lecturerId);

            if (lecturer != null)
            {
                ViewBag.FullName = lecturer.FullName;
            }

            // Tính toán thống kê
            var totalCourses = await _context.Courses
                .Where(c => c.LecturerID == lecturerId)
                .CountAsync();

            var totalStudents = await _context.Enrollments
                .Include(e => e.Course)
                .Where(e => e.Course.LecturerID == lecturerId)
                .Select(e => e.StudentID)
                .Distinct()
                .CountAsync();

            var pendingAssignments = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .Where(s => s.Assignment.LecturerID == lecturerId && (!s.Score.HasValue || s.Score == 0))
                .CountAsync();

            // Tính số lớp học hôm nay từ Timetable
            var today = DateTime.Today;
            var todayClasses = await _context.Timetable
                .Include(t => t.Course)
                .Where(t => t.Course.LecturerID == lecturerId && t.StudyDate == today)
                .CountAsync();

            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalStudents = totalStudents;
            ViewBag.PendingAssignments = pendingAssignments;
            ViewBag.TodayClasses = todayClasses;

            // Lấy danh sách môn học gần đây
            var courses = await _context.Courses
                .Include(c => c.Timetable)
                    .ThenInclude(t => t.Class)
                .Include(c => c.Enrollments)
                .Where(c => c.LecturerID == lecturerId)
                .OrderByDescending(c => c.CourseID)
                .Take(3)
                .ToListAsync();

            var recentCourses = courses.Select(c => new
            {
                c.CourseID,
                c.CourseName,
                ClassName = c.Timetable?.FirstOrDefault()?.Class?.ClassName ?? "N/A",
                StudentCount = c.Enrollments?.Count ?? 0
            }).ToList();

            ViewBag.RecentCourses = recentCourses;

            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView("_DashboardHomePartial");
            }

            return View();
        }

        // Action hiển thị profile giảng viên
        public async Task<IActionResult> Profile(bool partial = false)
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

            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView(lecturer);
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
        public async Task<IActionResult> Courses(bool partial = false)
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

            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView(courses);
            }

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
        public async Task<IActionResult> Assignments(bool partial = false)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId"); // ĐÃ SỬA
            if (!lecturerId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách bài tập của giảng viên
            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Lecturer)
                .Where(a => a.LecturerID == lecturerId.Value)
                .OrderByDescending(a => a.StartDate)
                .ToListAsync();

            // Đếm số bài nộp cho mỗi assignment
            var submissionCounts = new Dictionary<int, int>();
            var gradedCounts = new Dictionary<int, int>();

            foreach (var assignment in assignments)
            {
                var count = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentID == assignment.AssignmentID)
                    .CountAsync();

                var gradedCount = await _context.AssignmentSubmissions
                    .Where(s => s.AssignmentID == assignment.AssignmentID && s.Score.HasValue && s.Score > 0)
                    .CountAsync();

                submissionCounts[assignment.AssignmentID] = count;
                gradedCounts[assignment.AssignmentID] = gradedCount;
            }

            ViewBag.SubmissionCounts = submissionCounts;
            ViewBag.GradedCounts = gradedCounts;
            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView(assignments);
            }

            return View(assignments);
        }

        // GET: Trang chấm điểm
        public async Task<IActionResult> GradeAssignment(int id)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId"); // ĐÃ SỬA
            if (!lecturerId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy thông tin bài tập
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Lecturer)
                .FirstOrDefaultAsync(a => a.AssignmentID == id && a.LecturerID == lecturerId.Value);

            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài tập hoặc bạn không có quyền truy cập!";
                return RedirectToAction("Assignments");
            }

            // Lấy danh sách bài nộp
            var submissions = await _context.AssignmentSubmissions
                .Include(s => s.Student)
                .Include(s => s.Assignment)
                .Where(s => s.AssignmentID == id)
                .OrderByDescending(s => s.SubmitDate)
                .ToListAsync();

            ViewBag.Submissions = submissions;

            return View(assignment);
        }

        // POST: Lưu điểm
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveGrade(int submissionId, int assignmentId, decimal score, string feedback)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId"); // ĐÃ SỬA
            if (!lecturerId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Validate điểm
            if (score < 0 || score > 10)
            {
                TempData["ErrorMessage"] = "Điểm phải nằm trong khoảng từ 0 đến 10!";
                return RedirectToAction("GradeAssignment", new { id = assignmentId });
            }

            // Kiểm tra quyền sở hữu assignment
            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignmentID == assignmentId && a.LecturerID == lecturerId.Value);

            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền chấm điểm bài tập này!";
                return RedirectToAction("Assignments");
            }

            // Lấy submission
            var submission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.SubmissionID == submissionId);

            if (submission == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài nộp!";
                return RedirectToAction("GradeAssignment", new { id = assignmentId });
            }

            // Cập nhật điểm và nhận xét
            submission.Score = (double?)score;
            submission.Feedback = feedback;

            _context.AssignmentSubmissions.Update(submission);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã chấm điểm thành công cho sinh viên!";
            return RedirectToAction("GradeAssignment", new { id = assignmentId });
        }

        // GET: Download file bài nộp của sinh viên
        public async Task<IActionResult> DownloadSubmissionFile(int id)
        {
            // Kiểm tra đăng nhập
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId"); // ĐÃ SỬA
            if (!lecturerId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy submission và kiểm tra quyền
            var submission = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                .FirstOrDefaultAsync(s => s.SubmissionID == id);

            if (submission == null || submission.Assignment?.LecturerID != lecturerId.Value)
            {
                TempData["ErrorMessage"] = "Không tìm thấy file hoặc bạn không có quyền truy cập!";
                return RedirectToAction("Assignments");
            }

            if (string.IsNullOrEmpty(submission.FilePath))
            {
                TempData["ErrorMessage"] = "Bài nộp không có file đính kèm!";
                return RedirectToAction("GradeAssignment", new { id = submission.AssignmentID });
            }

            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", submission.FilePath.TrimStart('/'));

            if (!System.IO.File.Exists(filePath))
            {
                TempData["ErrorMessage"] = "File không tồn tại!";
                return RedirectToAction("GradeAssignment", new { id = submission.AssignmentID });
            }

            var fileName = Path.GetFileName(filePath);
            var fileBytes = await System.IO.File.ReadAllBytesAsync(filePath);
            var fileExtension = Path.GetExtension(fileName).ToLower();

            string contentType = fileExtension switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                ".xls" => "application/vnd.ms-excel",
                ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                ".zip" => "application/zip",
                ".rar" => "application/x-rar-compressed",
                _ => "application/octet-stream"
            };

            return File(fileBytes, contentType, fileName);
        }

        // GET: Tạo bài tập mới (placeholder)
        public IActionResult CreateAssignment()
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            // TODO: Implement create assignment form
            TempData["ErrorMessage"] = "Chức năng đang được phát triển!";
            return RedirectToAction("Assignments");
        }

        // GET: Sửa bài tập (placeholder)
        public IActionResult EditAssignment(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            // TODO: Implement edit assignment form
            TempData["ErrorMessage"] = "Chức năng đang được phát triển!";
            return RedirectToAction("Assignments");
        }

        // POST: Xóa bài tập
        [HttpGet]
        public async Task<IActionResult> DeleteAssignment(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId"); // ĐÃ SỬA
            if (!lecturerId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignmentID == id && a.LecturerID == lecturerId.Value);

            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài tập hoặc bạn không có quyền xóa!";
                return RedirectToAction("Assignments");
            }

            // Xóa các submissions liên quan
            var submissions = await _context.AssignmentSubmissions
                .Where(s => s.AssignmentID == id)
                .ToListAsync();

            // Xóa files
            foreach (var submission in submissions)
            {
                if (!string.IsNullOrEmpty(submission.FilePath))
                {
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", submission.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                }
            }

            _context.AssignmentSubmissions.RemoveRange(submissions);
            _context.Assignments.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Đã xóa bài tập thành công!";
            return RedirectToAction("Assignments");
        }

        // Action quản lý điểm danh
        public async Task<IActionResult> Attendance(bool partial = false)
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
            var courses = await _context.Courses
                .Where(c => c.LecturerID == lecturerId)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView();
            }

            return View();
        }

        // Action quản lý sinh viên
        public async Task<IActionResult> Students(bool partial = false)
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

            // Lấy danh sách sinh viên từ các môn học mà giảng viên dạy
            var students = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .Where(e => e.Course.LecturerID == lecturerId)
                .Select(e => e.Student)
                .Distinct()
                .ToListAsync();

            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView("InformationStudent", students);
            }

            return View("InformationStudent", students);
        }

        // Add this private method to LecturerController to fix CS0103
        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(e => e.LecturerID == id);
        }
    }
}