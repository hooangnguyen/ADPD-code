using ADPD_code.Data;
using ADPD_code.Models;
using ADPD_code.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADPD_code.Controllers
{
    public class LecturerController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ILecturerAnalyticsService _lecturerAnalyticsService;

        public LecturerController(ApplicationDbContext context, ILecturerAnalyticsService lecturerAnalyticsService)
        {
            _context = context;
            _lecturerAnalyticsService = lecturerAnalyticsService;
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

            // Lấy thống kê từ service
            var stats = await _lecturerAnalyticsService.GetDashboardStats(lecturerId.Value);
            ViewBag.TotalCourses = stats.TotalCourses;
            ViewBag.TotalStudents = stats.TotalStudents;
            ViewBag.PendingAssignments = stats.PendingAssignments;
            ViewBag.TodayClasses = stats.TodayClasses;

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

            // Lấy bài nộp gần đây (5 bài mới nhất)
            var recentSubmissions = await _context.AssignmentSubmissions
                .Include(s => s.Assignment)
                    .ThenInclude(a => a.Course)
                .Include(s => s.Student)
                    .ThenInclude(st => st.StudentClasses)
                        .ThenInclude(sc => sc.Class)
                .Where(s => s.Assignment.LecturerID == lecturerId)
                .OrderByDescending(s => s.SubmitDate)
                .Take(5)
                .ToListAsync();

            // Format dữ liệu bài nộp
            var formattedSubmissions = recentSubmissions.Select(s => new
            {
                AssignmentTitle = s.Assignment?.Title ?? "N/A",
                StudentName = s.Student?.FullName ?? "N/A",
                ClassName = s.Student?.StudentClasses?.FirstOrDefault()?.Class?.ClassName ?? "N/A",
                TimeAgo = GetTimeAgo(s.SubmitDate),
                Score = s.Score,
                Status = s.Score.HasValue && s.Score > 0 ? "graded" : "pending",
                StatusText = s.Score.HasValue && s.Score > 0 ? $"Đã chấm: {s.Score:F1}" : "Chưa chấm"
            }).ToList();

            ViewBag.RecentSubmissions = formattedSubmissions;

            // Lấy lịch dạy hôm nay
            var today = DateTime.Today;
            var todaySchedule = await _context.Timetable
                .Include(t => t.Course)
                .Include(t => t.Class)
                .Where(t => t.Course.LecturerID == lecturerId && t.StudyDate.Date == today)
                .OrderBy(t => t.StartTime)
                .Select(t => new
                {
                    Time = $"{t.StartTime:HH:mm} - {t.EndTime:HH:mm}",
                    CourseName = t.Course.CourseName,
                    ClassName = t.Class.ClassName,
                    Room = t.Room ?? "Chưa xác định"
                })
                .ToListAsync();

            ViewBag.TodaySchedule = todaySchedule;

            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView("_DashboardHomePartial");
            }

            return View();
        }

        // Helper method để tính thời gian đã trôi qua
        private string GetTimeAgo(DateTime? dateTime)
        {
            if (!dateTime.HasValue)
                return "Không xác định";

            var timeSpan = DateTime.Now - dateTime.Value;

            if (timeSpan.TotalMinutes < 1)
                return "Vừa xong";
            if (timeSpan.TotalMinutes < 60)
                return $"{(int)timeSpan.TotalMinutes} phút trước";
            if (timeSpan.TotalHours < 24)
                return $"{(int)timeSpan.TotalHours} giờ trước";
            if (timeSpan.TotalDays < 7)
                return $"{(int)timeSpan.TotalDays} ngày trước";
            if (timeSpan.TotalDays < 30)
                return $"{(int)(timeSpan.TotalDays / 7)} tuần trước";
            if (timeSpan.TotalDays < 365)
                return $"{(int)(timeSpan.TotalDays / 30)} tháng trước";
            
            return $"{(int)(timeSpan.TotalDays / 365)} năm trước";
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

        // GET: Tạo bài tập mới
        public async Task<IActionResult> CreateAssignment()
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            if (!lecturerId.HasValue)
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách môn học của giảng viên
            var courses = await _context.Courses
                .Where(c => c.LecturerID == lecturerId.Value)
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            ViewBag.Courses = courses;

            return View();
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
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            ViewBag.Courses = courses;
            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView("Attendance");
            }

            return View("Attendance");
        }

        // Action lấy danh sách sinh viên trong môn học để điểm danh
        [HttpGet]
        public async Task<IActionResult> GetStudentsForAttendance(int courseId, DateTime? date = null)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            if (lecturerId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            // Kiểm tra môn học thuộc về giảng viên
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == courseId && c.LecturerID == lecturerId);

            if (course == null)
            {
                return Json(new { success = false, message = "Môn học không tồn tại hoặc không thuộc quyền quản lý" });
            }

            // Lấy danh sách sinh viên đã đăng ký môn học
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                .Where(e => e.CourseID == courseId)
                .OrderBy(e => e.Student.FullName)
                .ToListAsync();

            var attendanceDate = date ?? DateTime.Today;

            // Lấy điểm danh đã có trong ngày (nếu có)
            var existingAttendance = await _context.Attendances
                .Where(a => a.CourseID == courseId && a.Date.Date == attendanceDate.Date)
                .ToListAsync();

            var students = enrollments.Select(e => new
            {
                studentId = e.StudentID,
                studentCode = e.StudentID.ToString().PadLeft(10, '0'),
                fullName = e.Student.FullName,
                email = e.Student.Email,
                status = existingAttendance.FirstOrDefault(a => a.StudentID == e.StudentID)?.Status ?? "Chưa điểm danh"
            }).ToList();

            return Json(new
            {
                success = true,
                courseName = course.CourseName,
                date = attendanceDate.ToString("yyyy-MM-dd"),
                students = students
            });
        }

        // Action lưu điểm danh
        [HttpPost]
        public async Task<IActionResult> SaveAttendance([FromBody] SaveAttendanceRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            if (lecturerId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            if (request == null || request.Records == null)
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ" });
            }

            // Kiểm tra môn học thuộc về giảng viên
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == request.CourseId && c.LecturerID == lecturerId);

            if (course == null)
            {
                return Json(new { success = false, message = "Môn học không tồn tại hoặc không thuộc quyền quản lý" });
            }

            try
            {
                var attendanceDate = request.Date.Date;

                // Xóa điểm danh cũ trong ngày (nếu có)
                var existingAttendance = await _context.Attendances
                    .Where(a => a.CourseID == request.CourseId && a.Date.Date == attendanceDate)
                    .ToListAsync();

                _context.Attendances.RemoveRange(existingAttendance);

                // Thêm điểm danh mới
                foreach (var record in request.Records)
                {
                    if (!string.IsNullOrEmpty(record.Status) && record.Status != "Chưa điểm danh")
                    {
                        var attendance = new Attendance
                        {
                            StudentID = record.StudentId,
                            CourseID = request.CourseId,
                            Date = attendanceDate,
                            Status = record.Status
                        };

                        _context.Attendances.Add(attendance);
                    }
                }

                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Lưu điểm danh thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Action xem lịch sử điểm danh
        [HttpGet]
        public async Task<IActionResult> GetAttendanceHistory(int courseId, DateTime? startDate = null, DateTime? endDate = null)
        {
            if (HttpContext.Session.GetString("Role") != "Lecturer")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var lecturerId = HttpContext.Session.GetInt32("LecturerId");
            if (lecturerId == null)
            {
                return Json(new { success = false, message = "Chưa đăng nhập" });
            }

            // Kiểm tra môn học thuộc về giảng viên
            var course = await _context.Courses
                .FirstOrDefaultAsync(c => c.CourseID == courseId && c.LecturerID == lecturerId);

            if (course == null)
            {
                return Json(new { success = false, message = "Môn học không tồn tại hoặc không thuộc quyền quản lý" });
            }

            var query = _context.Attendances
                .Include(a => a.Student)
                .Where(a => a.CourseID == courseId);

            if (startDate.HasValue)
            {
                query = query.Where(a => a.Date >= startDate.Value.Date);
            }

            if (endDate.HasValue)
            {
                query = query.Where(a => a.Date <= endDate.Value.Date);
            }

            var attendanceList = await query
                .OrderByDescending(a => a.Date)
                .ThenBy(a => a.Student.FullName)
                .ToListAsync();

            var history = attendanceList
                .GroupBy(a => a.Date)
                .Select(g => new
                {
                    date = g.Key.ToString("yyyy-MM-dd"),
                    dateDisplay = g.Key.ToString("dd/MM/yyyy"),
                    records = g.Select(a => new
                    {
                        studentId = a.StudentID,
                        studentName = a.Student.FullName,
                        status = a.Status
                    }).ToList()
                }).ToList();

            return Json(new
            {
                success = true,
                courseName = course.CourseName,
                history = history
            });
        }

        // Add this private method to LecturerController to fix CS0103
        private bool LecturerExists(int id)
        {
            return _context.Lecturers.Any(e => e.LecturerID == id);
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

            // Lấy danh sách sinh viên từ các môn học mà giảng viên dạy với thông tin đầy đủ
            var enrollments = await _context.Enrollments
                .Include(e => e.Student)
                    .ThenInclude(s => s.StudentClasses)
                        .ThenInclude(sc => sc.Class)
                .Include(e => e.Course)
                .Where(e => e.Course.LecturerID == lecturerId)
                .ToListAsync();

            // Tạo danh sách sinh viên với thông tin chi tiết
            var studentList = enrollments
                .Where(e => e.Student != null)
                .GroupBy(e => e.StudentID)
                .Select(g => new
                {
                    Student = g.First().Student,
                    Enrollments = g.ToList(),
                    Classes = g.First().Student?.StudentClasses?
                        .Where(sc => sc.Class != null)
                        .Select(sc => sc.Class!.ClassName)
                        .Where(c => !string.IsNullOrEmpty(c))
                        .Distinct()
                        .ToList() ?? new List<string?>(),
                    GPA = g.Where(e => e.Score.HasValue).Any() 
                        ? Math.Round(g.Where(e => e.Score.HasValue).Average(e => e.Score!.Value), 2) 
                        : 0.0,
                    TotalAttendance = _context.Attendances
                        .Count(a => a.StudentID == g.Key && a.Status == "Có mặt"),
                    TotalAbsent = _context.Attendances
                        .Count(a => a.StudentID == g.Key && a.Status == "Vắng"),
                    TotalDays = _context.Attendances
                        .Count(a => a.StudentID == g.Key)
                })
                .Where(s => s.Student != null)
                .ToList();

            // Tính attendance rate cho mỗi sinh viên
            var studentsWithStats = studentList.Select(s => new
            {
                StudentId = s.Student?.StudentId ?? 0,
                MSSV = s.Student?.StudentId.ToString().PadLeft(10, '0') ?? "N/A",
                FullName = s.Student?.FullName ?? "N/A",
                Email = s.Student?.Email ?? "N/A",
                Phone = s.Student?.Phone ?? "N/A",
                DOB = s.Student?.DOB.ToString("dd/MM/yyyy") ?? "N/A",
                Gender = s.Student?.Gender ?? "N/A",
                Address = s.Student?.Address ?? "N/A",
                Status = s.Student?.Status ?? "N/A",
                ClassName = s.Classes.FirstOrDefault() ?? "N/A",
                AllClasses = s.Classes,
                GPA = s.GPA,
                AttendanceRate = s.TotalDays > 0 
                    ? Math.Round((double)s.TotalAttendance / s.TotalDays * 100, 1) 
                    : 0.0,
                TotalAbsent = s.TotalAbsent
            }).ToList();

            // Lấy danh sách lớp để filter
            var allClasses = studentsWithStats
                .SelectMany(s => s.AllClasses)
                .Where(c => !string.IsNullOrEmpty(c))
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            ViewBag.Students = studentsWithStats;
            ViewBag.Classes = allClasses;
            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView("InformationStudent");
            }

            return View("InformationStudent");
        }
    }

    // ViewModel cho điểm danh
    public class AttendanceRecord
    {
        public int StudentId { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    // Request model cho lưu điểm danh
    public class SaveAttendanceRequest
    {
        public int CourseId { get; set; }
        public DateTime Date { get; set; }
        public List<AttendanceRecord> Records { get; set; } = new List<AttendanceRecord>();
    }
}