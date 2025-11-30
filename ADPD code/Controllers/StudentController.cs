using ADPD_code.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ADPD_code.ViewModels;

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
            var tomorrow = today.AddDays(1);
            
            // Lấy ClassID của sinh viên từ StudentClass
            var studentClassIds = await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId.Value)
                .Select(sc => sc.ClassID)
                .ToListAsync();

            // Lấy lịch học hôm nay từ Timetable
            // Sử dụng so sánh range để tránh vấn đề với .Date trong EF Core
            var todaySchedule = await _context.Timetable
                .Include(t => t.Course)
                .ThenInclude(c => c.Lecturer)
                .Where(t => studentClassIds.Contains(t.ClassID) 
                    && t.StudyDate >= today 
                    && t.StudyDate < tomorrow)
                .OrderBy(t => t.StartTime)
                .Select(t => new
                {
                    CourseName = t.Course != null ? t.Course.CourseName : "N/A",
                    CourseCode = t.Course != null ? $"CS{t.Course.CourseID}" : "N/A",
                    Time = $"{t.StartTime:HH\\:mm} - {t.EndTime:HH\\:mm}", // Format 24h
                    Room = t.Room ?? "N/A",
                    LecturerName = t.Course != null && t.Course.Lecturer != null ? t.Course.Lecturer.FullName : "N/A",
                    StudyDate = t.StudyDate
                })
                .ToListAsync();

            ViewBag.TodaySchedule = todaySchedule;
            ViewBag.TodayClasses = todaySchedule.Count;
            ViewBag.TodayDate = today; // Thêm ngày hiện tại vào ViewBag

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
        public async Task<IActionResult> Grades(bool partial = false)
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

            // Lấy danh sách điểm từ bảng Enrollment với Include Course và Lecturer
            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                    .ThenInclude(c => c.Lecturer)
                .Where(e => e.StudentID == studentId.Value)
                .OrderByDescending(e => e.AcademicYear)
                .ThenByDescending(e => e.Semester)
                .ToListAsync();

            ViewBag.StudentId = studentId.Value;
            ViewData["IsPartial"] = partial;

            return partial ? PartialView(enrollments) : View(enrollments);
        }

        // Schedule - Lịch học
        public async Task<IActionResult> Schedule(bool partial = false)
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

            // Lấy ClassID của sinh viên
            var studentClassIds = await _context.StudentClasses
                .Where(sc => sc.StudentId == studentId.Value)
                .Select(sc => sc.ClassID)
                .ToListAsync();

            // Lấy lịch học từ Timetable
            var timetable = await _context.Timetable
                .Include(t => t.Course)
                    .ThenInclude(c => c.Lecturer)
                .Where(t => studentClassIds.Contains(t.ClassID))
                .OrderBy(t => t.StudyDate)
                .ThenBy(t => t.StartTime)
                .ToListAsync();

            // Lấy danh sách Course từ Timetable
            var courses = timetable
                .Where(t => t.Course != null)
                .Select(t => t.Course!)
                .Distinct()
                .OrderBy(c => c.CourseName)
                .ToList();

            // Truyền thêm thông tin Timetable để hiển thị lịch chi tiết
            ViewBag.Timetable = timetable;
            ViewBag.StudentId = studentId.Value;
            ViewData["IsPartial"] = partial;

            return partial ? PartialView(courses) : View(courses);
        }

        // Assignments - Bài tập
        public async Task<IActionResult> Assignments(bool partial = false)
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

            var studentCourseIds = await _context.Enrollments
                .Where(e => e.StudentID == studentId.Value)
                .Select(e => e.CourseID)
                .ToListAsync();

            var assignments = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Lecturer)
                .Where(a => studentCourseIds.Contains(a.CourseID))
                .OrderByDescending(a => a.EndDate)
                .ToListAsync();

            var submissions = await _context.AssignmentSubmissions
                .Where(s => s.StudentID == studentId.Value)
                .ToListAsync();

            ViewBag.StudentId = studentId.Value;
            ViewBag.Submissions = submissions;
            ViewData["IsPartial"] = partial;

            return partial ? PartialView(assignments) : View(assignments);
        }

        // Subject - Danh sách môn học
        public async Task<IActionResult> Subject(bool partial = false)
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

            var enrollments = await _context.Enrollments
                .Include(e => e.Course)
                .ThenInclude(c => c.Lecturer)
                .Include(e => e.Student)
                .ThenInclude(s => s.StudentClasses)
                .ThenInclude(sc => sc.Class)
                .Where(e => e.StudentID == studentId.Value)
                .OrderByDescending(e => e.AcademicYear)
                .ThenByDescending(e => e.Semester)
                .ToListAsync();

            var model = enrollments.Select(e => new StudentSubjectViewModel
            {
                CourseId = e.CourseID,
                CourseName = e.Course?.CourseName ?? "N/A",
                CourseCode = $"CS{e.CourseID}",
                Credits = e.Course?.Credits ?? 0,
                LecturerName = e.Course?.Lecturer?.FullName ?? "Chưa phân công",
                Semester = e.Semester ?? "N/A",
                AcademicYear = e.AcademicYear ?? "N/A",
                Score = e.Score,
                ClassName = e.Student?.StudentClasses?.FirstOrDefault()?.Class?.ClassName ?? "N/A"
            }).ToList();

            ViewData["IsPartial"] = partial;
            return partial ? PartialView(model) : View(model);
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
        public async Task<IActionResult> SubmitAssignment(int id, bool partial = false)
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

            // Lấy thông tin bài tập
            var assignment = await _context.Assignments
                .Include(a => a.Course)
                .Include(a => a.Lecturer)
                .FirstOrDefaultAsync(a => a.AssignmentID == id);

            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài tập!";
                return RedirectToAction("Assignments");
            }

            // Kiểm tra xem sinh viên đã nộp bài chưa
            var submission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentID == id && s.StudentID == studentId.Value);

            ViewBag.Submission = submission;
            ViewData["IsPartial"] = partial;

            return View(assignment);
        }

        // POST: Xử lý nộp bài
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitAssignment(int assignmentId, IFormFile file, string submissionText)
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

            // Validate assignment exists
            var assignment = await _context.Assignments.FindAsync(assignmentId);
            if (assignment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy bài tập!";
                return RedirectToAction("Assignments");
            }

            // Check deadline
            if (DateTime.Now > assignment.EndDate)
            {
                TempData["ErrorMessage"] = "Đã quá hạn nộp bài!";
                return RedirectToAction("SubmitAssignment", new { id = assignmentId });
            }

            // Kiểm tra xem đã nộp bài chưa
            var existingSubmission = await _context.AssignmentSubmissions
                .FirstOrDefaultAsync(s => s.AssignmentID == assignmentId && s.StudentID == studentId.Value);

            // Validate: Nếu chưa submit lần nào thì phải có file
            if (existingSubmission == null && (file == null || file.Length == 0))
            {
                TempData["ErrorMessage"] = "Vui lòng chọn file để nộp bài!";
                return RedirectToAction("SubmitAssignment", new { id = assignmentId });
            }

            string filePath = existingSubmission?.FilePath;

            // Xử lý upload file
            if (file != null && file.Length > 0)
            {
                // Validate file type
                var allowedExtensions = new[] { ".doc", ".docx", ".xls", ".xlsx", ".pdf", ".zip", ".rar" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    TempData["ErrorMessage"] = "Định dạng file không được hỗ trợ! Chỉ chấp nhận: Word, Excel, PDF, ZIP";
                    return RedirectToAction("SubmitAssignment", new { id = assignmentId });
                }

                // Validate file size (10MB max)
                if (file.Length > 10 * 1024 * 1024)
                {
                    TempData["ErrorMessage"] = "File quá lớn! Kích thước tối đa là 10MB";
                    return RedirectToAction("SubmitAssignment", new { id = assignmentId });
                }

                // Tạo thư mục lưu file nếu chưa có
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "assignments");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file unique
                var fileName = $"{studentId.Value}_{assignmentId}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                var fullPath = Path.Combine(uploadsFolder, fileName);

                // Xóa file cũ nếu có
                if (!string.IsNullOrEmpty(existingSubmission?.FilePath))
                {
                    var oldFilePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", existingSubmission.FilePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldFilePath))
                    {
                        System.IO.File.Delete(oldFilePath);
                    }
                }

                // Lưu file mới
                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                filePath = $"/uploads/assignments/{fileName}";
            }

            // Lưu submission mới hoặc cập nhật submission cũ
            if (existingSubmission == null)
            {
                var submission = new Models.AssignmentSubmission
                {
                    AssignmentID = assignmentId,
                    StudentID = studentId.Value,
                    FilePath = filePath,
                    SubmissionText = !string.IsNullOrWhiteSpace(submissionText) ? submissionText : null,
                    SubmitDate = DateTime.Now
                };
                _context.AssignmentSubmissions.Add(submission);
            }
            else
            {
                // Chỉ cập nhật filePath nếu có file mới
                if (!string.IsNullOrEmpty(filePath))
                {
                    existingSubmission.FilePath = filePath;
                }
                // Cập nhật nội dung nếu có
                if (!string.IsNullOrWhiteSpace(submissionText))
                {
                    existingSubmission.SubmissionText = submissionText;
                }
                existingSubmission.SubmitDate = DateTime.Now;
                _context.AssignmentSubmissions.Update(existingSubmission);
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Nộp bài thành công!";
            return RedirectToAction("Assignments");
        }
    }
}