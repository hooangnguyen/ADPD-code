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

            if (!studentId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

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

            ViewBag.Username = username;

            // ========== TÍNH TOÁN DỮ LIỆU THỐNG KÊ ==========

            // 1. Lấy tất cả Enrollment của sinh viên
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Lecturer)
                .Where(e => e.StudentID == studentId.Value)
                .ToListAsync();

            // 2. Tính GPA (trung bình điểm)
            var scores = enrollments
                .Where(e => e.Score.HasValue)
                .Select(e => e.Score.Value)
                .ToList();

            var gpa = scores.Any() ? Math.Round(scores.Average(), 2) : 0.0;
            ViewBag.GPA = gpa;

            // 3. Tính tổng tín chỉ đã học
            var totalCredits = enrollments
                .Where(e => e.Course != null)
                .Sum(e => e.Course.Credits);
            ViewBag.TotalCredits = totalCredits;
            ViewBag.MaxCredits = 120; // Tổng tín chỉ tối đa

            // 4. Lấy học kỳ hiện tại (giả sử là học kỳ gần nhất)
            var currentSemester = enrollments
                .Where(e => !string.IsNullOrEmpty(e.Semester))
                .OrderByDescending(e => e.AcademicYear)
                .ThenByDescending(e => e.Semester)
                .Select(e => e.Semester)
                .FirstOrDefault() ?? "HK1";

            var currentYear = enrollments
                .Where(e => !string.IsNullOrEmpty(e.AcademicYear))
                .OrderByDescending(e => e.AcademicYear)
                .Select(e => e.AcademicYear)
                .FirstOrDefault() ?? DateTime.Now.Year.ToString();

            // 5. Số môn học kỳ này
            var currentSemesterCourses = enrollments
                .Count(e => e.Semester == currentSemester && e.AcademicYear == currentYear);
            ViewBag.CurrentSemesterCourses = currentSemesterCourses;

            // 6. Lịch học hôm nay từ bảng Timetable
            var today = DateTime.Now.Date;
            
            // Lấy ClassID của sinh viên từ StudentClass
            var studentClassIds = await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId.Value)
                .Select(sc => sc.ClassID)
                .ToListAsync();

            // Lấy lịch học hôm nay từ Timetable
            var todaySchedule = await _context.Timetable
                .Include(t => t.Course)
                .ThenInclude(c => c.Lecturer)
                .Where(t => studentClassIds.Contains(t.ClassID) && t.StudyDate.Date == today)
                .OrderBy(t => t.StartTime)
                .Select(t => new
                {
                    CourseName = t.Course != null ? t.Course.CourseName : "N/A",
                    CourseCode = t.Course != null ? $"CS{t.Course.CourseID}" : "N/A",
                    Time = $"{t.StartTime:hh\\:mm} - {t.EndTime:hh\\:mm}",
                    Room = t.Room ?? "N/A",
                    LecturerName = t.Course != null && t.Course.Lecturer != null ? t.Course.Lecturer.FullName : "N/A"
                })
                .ToListAsync();

            ViewBag.TodaySchedule = todaySchedule;
            ViewBag.TodayClasses = todaySchedule.Count;

            // 7. Bài tập gần đây (lấy từ Assignment của các môn học sinh viên đã đăng ký)
            var studentCourseIds = enrollments
                .Where(e => e.CourseID > 0)
                .Select(e => e.CourseID)
                .ToList();

            var recentAssignments = await _context.Assignments
                .Include(a => a.Course)
                .Where(a => studentCourseIds.Contains(a.CourseID))
                .OrderByDescending(a => a.EndDate)
                .Take(5)
                .Select(a => new
                {
                    Title = a.Title ?? "N/A",
                    CourseName = a.Course != null ? a.Course.CourseName : "N/A",
                    Deadline = a.EndDate,
                    DaysLeft = (int)(a.EndDate.Date - today).TotalDays
                })
                .ToListAsync();

            ViewBag.RecentAssignments = recentAssignments;

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

            // Example: If you use student.StudentClasses.First().Class.Major somewhere, check for nulls:
            // var majorName = student.StudentClasses.FirstOrDefault()?.Class?.Major?.MajorName;

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

        // RegisterStudy - Đăng ký môn học
        public async Task<IActionResult> RegisterStudy(bool partial = false, string search = "", string credits = "", string lecturer = "")
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

            // Lấy danh sách môn học đã đăng ký của sinh viên
            var registeredCourseIds = await _context.Enrollments
                .Where(e => e.StudentID == studentId.Value)
                .Select(e => e.CourseID)
                .ToListAsync();

            ViewBag.RegisteredCourseIds = registeredCourseIds;

            // Lấy học kỳ hiện tại
            var currentEnrollment = await _context.Enrollments
                .Where(e => e.StudentID == studentId.Value)
                .OrderByDescending(e => e.AcademicYear)
                .ThenByDescending(e => e.Semester)
                .FirstOrDefaultAsync();

            var currentSemester = currentEnrollment?.Semester ?? "HK1";
            var currentYear = currentEnrollment?.AcademicYear ?? $"{DateTime.Now.Year}-{DateTime.Now.Year + 1}";

            ViewBag.CurrentSemester = currentSemester;
            ViewBag.CurrentAcademicYear = currentYear;

            // Lấy danh sách tất cả môn học với filter
            var coursesQuery = _context.Courses
                .Include(c => c.Lecturer)
                .Include(c => c.Enrollments)
                .AsQueryable();

            // Filter theo tên môn học
            if (!string.IsNullOrEmpty(search))
            {
                coursesQuery = coursesQuery.Where(c => 
                    c.CourseName != null && c.CourseName.Contains(search));
            }

            // Filter theo số tín chỉ
            if (!string.IsNullOrEmpty(credits) && int.TryParse(credits, out int creditValue))
            {
                coursesQuery = coursesQuery.Where(c => c.Credits == creditValue);
            }

            // Filter theo giảng viên
            if (!string.IsNullOrEmpty(lecturer))
            {
                coursesQuery = coursesQuery.Where(c => 
                    c.Lecturer != null && 
                    c.Lecturer.FullName != null && 
                    c.Lecturer.FullName.Contains(lecturer));
            }

            var courses = await coursesQuery
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            ViewData["IsPartial"] = partial;
            return partial ? PartialView(courses) : View(courses);
        }

        // RegisterCourse - Xử lý đăng ký môn học (POST)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegisterCourse(int courseId, string semester, string academicYear, bool partial = false)
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

            // Kiểm tra môn học đã đăng ký chưa
            var existingEnrollment = await _context.Enrollments
                .FirstOrDefaultAsync(e => e.StudentID == studentId.Value && e.CourseID == courseId);

            if (existingEnrollment != null)
            {
                TempData["ErrorMessage"] = "Bạn đã đăng ký môn học này rồi!";
                return RedirectToAction("RegisterStudy", new { partial = partial });
            }

            // Kiểm tra môn học có tồn tại không
            var course = await _context.Courses.FindAsync(courseId);
            if (course == null)
            {
                TempData["ErrorMessage"] = "Môn học không tồn tại!";
                return RedirectToAction("RegisterStudy", new { partial = partial });
            }

            // Tạo enrollment mới
            var enrollment = new Models.Enrollment
            {
                StudentID = studentId.Value,
                CourseID = courseId,
                Semester = semester,
                AcademicYear = academicYear,
                Score = null // Chưa có điểm
            };

            _context.Enrollments.Add(enrollment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đăng ký môn học {course.CourseName} thành công!";
            return RedirectToAction("RegisterStudy", new { partial = partial });
        }
    }
}