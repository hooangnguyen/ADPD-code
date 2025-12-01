using ADPD_code.Data;
using ADPD_code.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace ADPD_code.Controllers
{
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Dashboard - Trang chủ Admin
        public async Task<IActionResult> Dashboard(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            // Thống kê tổng quan
            var totalStudents = await _context.Students.CountAsync();
            var totalLecturers = await _context.Lecturers.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalClasses = await _context.Classes.CountAsync();
            var totalAccounts = await _context.Accounts.CountAsync();
            var activeEnrollments = await _context.Enrollments.CountAsync();

            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalLecturers = totalLecturers;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.TotalAccounts = totalAccounts;
            ViewBag.ActiveEnrollments = activeEnrollments;

            ViewData["IsPartial"] = partial;

            if (partial)
            {
                return PartialView("_DashboardHomePartial");
            }

            return View();
        }

        // ========== QUẢN LÝ SINH VIÊN ==========
        public async Task<IActionResult> Students(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var students = await _context.Students
                .Include(s => s.StudentClasses)
                    .ThenInclude(sc => sc.Class)
                .OrderBy(s => s.StudentId)
                .ToListAsync();

            ViewBag.Classes = await _context.Classes.ToListAsync();
            ViewData["IsPartial"] = partial;

            return partial ? PartialView(students) : View(students);
        }

        [HttpGet]
        public async Task<IActionResult> CreateStudent()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            // Trả về JSON với danh sách lớp để frontend có thể sử dụng
            var classes = await _context.Classes.ToListAsync();
            return Json(new { success = true, classes = classes.Select(c => new { ClassID = c.ClassID, ClassName = c.ClassName }) });
        }

        [HttpPost]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return Json(new { success = false, message = "Họ và tên không được để trống!" });
                }
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Json(new { success = false, message = "Email không được để trống!" });
                }
                if (string.IsNullOrWhiteSpace(request.DOB))
                {
                    return Json(new { success = false, message = "Ngày sinh không được để trống!" });
                }

                // Parse ngày sinh
                if (!DateTime.TryParse(request.DOB, out DateTime dob))
                {
                    return Json(new { success = false, message = "Ngày sinh không hợp lệ!" });
                }

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = await _context.Students
                    .FirstOrDefaultAsync(s => s.Email == request.Email);
                if (existingEmail != null)
                {
                    return Json(new { success = false, message = "Email đã tồn tại trong hệ thống!" });
                }

                var student = new Student
                {
                    FullName = request.FullName,
                    Gender = request.Gender ?? "Nam",
                    DOB = dob,
                    Email = request.Email,
                    Phone = request.Phone ?? "",
                    Address = request.Address ?? "",
                    Status = request.Status ?? "Đang học"
                };

                _context.Students.Add(student);
                await _context.SaveChangesAsync();

                // Thêm vào lớp nếu có
                if (request.ClassID.HasValue && request.ClassID.Value > 0)
                {
                    // Kiểm tra xem đã có trong lớp này chưa
                    var existingClass = await _context.StudentClasses
                        .FirstOrDefaultAsync(sc => sc.StudentId == student.StudentId && sc.ClassID == request.ClassID.Value);
                    
                    if (existingClass == null)
                    {
                        var studentClass = new StudentClass
                        {
                            StudentId = student.StudentId,
                            ClassID = request.ClassID.Value
                        };
                        _context.StudentClasses.Add(studentClass);
                        await _context.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, message = "Thêm sinh viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditStudent(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var student = await _context.Students
                .Include(s => s.StudentClasses)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sinh viên" });
            }

            var classId = student.StudentClasses?.FirstOrDefault()?.ClassID;

            return Json(new { 
                success = true, 
                student = new {
                    StudentId = student.StudentId,
                    FullName = student.FullName,
                    Gender = student.Gender,
                    DOB = student.DOB.ToString("yyyy-MM-dd"),
                    Email = student.Email,
                    Phone = student.Phone,
                    Address = student.Address,
                    Status = student.Status,
                    ClassID = classId
                }
            });
        }
        [HttpPost]
        public async Task<IActionResult> EditStudent(int id, [FromBody] EditStudentRequest? request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Kiểm tra ID hợp lệ
                if (id <= 0)
                {
                    return Json(new { success = false, message = "ID sinh viên không hợp lệ" });
                }

                // Nếu request null, đọc body thủ công
                if (request == null)
                {
                    Request.EnableBuffering();
                    Request.Body.Position = 0;
                    using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
                    {
                        var body = await reader.ReadToEndAsync();
                        Request.Body.Position = 0;

                        if (string.IsNullOrWhiteSpace(body))
                        {
                            return Json(new { success = false, message = "Request body rỗng. Vui lòng kiểm tra lại dữ liệu gửi lên." });
                        }

                        try
                        {
                            request = JsonSerializer.Deserialize<EditStudentRequest>(body, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        catch (Exception jsonEx)
                        {
                            return Json(new { success = false, message = $"Lỗi deserialize JSON: {jsonEx.Message}" });
                        }
                    }
                }

                if (request == null)
                {
                    return Json(new { success = false, message = "Không thể đọc dữ liệu. Vui lòng thử lại." });
                }

                // Tìm sinh viên
                var student = await _context.Students.FindAsync(id);
                if (student == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy sinh viên" });
                }

                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return Json(new { success = false, message = "Họ và tên không được để trống!" });
                }
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Json(new { success = false, message = "Email không được để trống!" });
                }
                if (string.IsNullOrWhiteSpace(request.DOB))
                {
                    return Json(new { success = false, message = "Ngày sinh không được để trống!" });
                }

                // Kiểm tra email đã tồn tại (trừ chính sinh viên này)
                var existingEmail = await _context.Students
                    .FirstOrDefaultAsync(s => s.Email == request.Email && s.StudentId != id);
                if (existingEmail != null)
                {
                    return Json(new { success = false, message = "Email đã tồn tại trong hệ thống!" });
                }

                // Parse ngày sinh
                if (!DateTime.TryParse(request.DOB, out DateTime dob))
                {
                    return Json(new { success = false, message = "Ngày sinh không hợp lệ!" });
                }

                // Cập nhật thông tin sinh viên
                student.FullName = request.FullName;
                student.Gender = request.Gender ?? student.Gender;
                student.DOB = dob;
                student.Email = request.Email;
                student.Phone = request.Phone ?? student.Phone;
                student.Address = request.Address ?? student.Address;
                student.Status = request.Status ?? student.Status;

                _context.Update(student);
                await _context.SaveChangesAsync();

                // Cập nhật lớp học nếu có
                if (request.ClassID.HasValue && request.ClassID.Value > 0)
                {
                    var existingClass = await _context.StudentClasses
                        .FirstOrDefaultAsync(sc => sc.StudentId == id);

                    if (existingClass != null)
                    {
                        existingClass.ClassID = request.ClassID.Value;
                        _context.Update(existingClass);
                    }
                    else
                    {
                        var studentClass = new StudentClass
                        {
                            StudentId = id,
                            ClassID = request.ClassID.Value
                        };
                        _context.StudentClasses.Add(studentClass);
                    }
                    await _context.SaveChangesAsync();
                }

                return Json(new { success = true, message = "Cập nhật sinh viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var student = await _context.Students
                .Include(s => s.StudentClasses)
                .Include(s => s.Enrollments)
                .Include(s => s.Attendances)
                .Include(s => s.AssignmentSubmissions)
                .FirstOrDefaultAsync(s => s.StudentId == id);

            if (student == null)
            {
                return Json(new { success = false, message = "Không tìm thấy sinh viên" });
            }

            try
            {
                // Xóa Account liên quan trước (vì có DeleteBehavior.Restrict)
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.StudentID == id);
                if (account != null)
                {
                    _context.Accounts.Remove(account);
                }

                // Xóa các bản ghi liên quan (mặc dù có cascade, nhưng để chắc chắn)
                // StudentClasses sẽ tự động xóa do cascade
                // Enrollments sẽ tự động xóa do cascade
                // Attendances sẽ tự động xóa do cascade
                // AssignmentSubmissions sẽ tự động xóa do cascade

                // Xóa sinh viên
                _context.Students.Remove(student);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Xóa sinh viên thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                // Lấy inner exception để có thông tin chi tiết hơn
                var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                return Json(new { success = false, message = $"Lỗi database: {innerEx}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ========== QUẢN LÝ GIẢNG VIÊN ==========
        public async Task<IActionResult> Lecturers(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var lecturers = await _context.Lecturers
                .Include(l => l.Department)
                .OrderBy(l => l.LecturerID)
                .ToListAsync();

            ViewBag.Departments = await _context.Departments.ToListAsync();
            ViewData["IsPartial"] = partial;

            return partial ? PartialView(lecturers) : View(lecturers);
        }

        [HttpPost]
        public async Task<IActionResult> CreateLecturer([FromBody] CreateLecturerRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return Json(new { success = false, message = "Họ và tên không được để trống!" });
                }
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Json(new { success = false, message = "Email không được để trống!" });
                }

                // Kiểm tra email đã tồn tại chưa
                var existingEmail = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email == request.Email);
                if (existingEmail != null)
                {
                    return Json(new { success = false, message = "Email đã tồn tại trong hệ thống!" });
                }

                var lecturer = new Lecturer
                {
                    FullName = request.FullName,
                    Email = request.Email,
                    Phone = request.Phone ?? "",
                    DepartmentID = request.DepartmentID ?? 0
                };

                _context.Lecturers.Add(lecturer);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm giảng viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditLecturer(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var lecturer = await _context.Lecturers
                .Include(l => l.Department)
                .FirstOrDefaultAsync(l => l.LecturerID == id);

            if (lecturer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy giảng viên" });
            }

            return Json(new { 
                success = true, 
                lecturer = new {
                    LecturerID = lecturer.LecturerID,
                    FullName = lecturer.FullName,
                    Email = lecturer.Email,
                    Phone = lecturer.Phone,
                    DepartmentID = lecturer.DepartmentID
                }
            });
        }
        [HttpPost]
        public async Task<IActionResult> EditLecturer(int id, [FromBody] EditLecturerRequest? request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Kiểm tra ID hợp lệ
                if (id <= 0)
                {
                    return Json(new { success = false, message = "ID giảng viên không hợp lệ" });
                }

                // Nếu request null, đọc body thủ công
                if (request == null)
                {
                    Request.EnableBuffering();
                    Request.Body.Position = 0;
                    using (var reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
                    {
                        var body = await reader.ReadToEndAsync();
                        Request.Body.Position = 0;

                        if (string.IsNullOrWhiteSpace(body))
                        {
                            return Json(new { success = false, message = "Request body rỗng. Vui lòng kiểm tra lại dữ liệu gửi lên." });
                        }

                        try
                        {
                            request = JsonSerializer.Deserialize<EditLecturerRequest>(body, new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            });
                        }
                        catch (Exception jsonEx)
                        {
                            return Json(new { success = false, message = $"Lỗi deserialize JSON: {jsonEx.Message}" });
                        }
                    }
                }

                if (request == null)
                {
                    return Json(new { success = false, message = "Không thể đọc dữ liệu. Vui lòng thử lại." });
                }

                // Tìm giảng viên
                var lecturer = await _context.Lecturers.FindAsync(id);
                if (lecturer == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy giảng viên" });
                }

                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(request.FullName))
                {
                    return Json(new { success = false, message = "Họ và tên không được để trống!" });
                }
                if (string.IsNullOrWhiteSpace(request.Email))
                {
                    return Json(new { success = false, message = "Email không được để trống!" });
                }

                // Kiểm tra email đã tồn tại (trừ chính giảng viên này)
                var existingEmail = await _context.Lecturers
                    .FirstOrDefaultAsync(l => l.Email == request.Email && l.LecturerID != id);
                if (existingEmail != null)
                {
                    return Json(new { success = false, message = "Email đã tồn tại trong hệ thống!" });
                }

                // Cập nhật thông tin giảng viên
                lecturer.FullName = request.FullName;
                lecturer.Email = request.Email;
                lecturer.Phone = request.Phone ?? lecturer.Phone;
                lecturer.DepartmentID = request.DepartmentID ?? lecturer.DepartmentID;

                _context.Update(lecturer);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật giảng viên thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }
        [HttpPost]
        public async Task<IActionResult> DeleteLecturer(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var lecturer = await _context.Lecturers
                .Include(l => l.Courses)
                .Include(l => l.Assignments)
                .FirstOrDefaultAsync(l => l.LecturerID == id);

            if (lecturer == null)
            {
                return Json(new { success = false, message = "Không tìm thấy giảng viên" });
            }

            try
            {
                // Xóa Account liên quan trước (vì có DeleteBehavior.Restrict)
                var account = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.LecturerID == id);
                if (account != null)
                {
                    _context.Accounts.Remove(account);
                }

                // Xóa giảng viên (Courses và Assignments sẽ tự động xóa do cascade)
                _context.Lecturers.Remove(lecturer);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Xóa giảng viên thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                return Json(new { success = false, message = $"Lỗi database: {innerEx}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ========== QUẢN LÝ LỚP HỌC ==========
        public async Task<IActionResult> Classes(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var classes = await _context.Classes
                .Include(c => c.Major)
                .Include(c => c.StudentClasses)
                .OrderBy(c => c.ClassName)
                .ToListAsync();

            ViewBag.Majors = await _context.Majors.ToListAsync();
            ViewData["IsPartial"] = partial;

            return partial ? PartialView(classes) : View(classes);
        }

        [HttpPost]
        public async Task<IActionResult> CreateClass()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                HttpContext.Request.EnableBuffering();
                HttpContext.Request.Body.Position = 0;

                string body;
                using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    body = await reader.ReadToEndAsync();
                }
                HttpContext.Request.Body.Position = 0;

                if (string.IsNullOrWhiteSpace(body))
                {
                    return Json(new { success = false, message = "Không có dữ liệu gửi lên" });
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<EditClassRequest>(body, options);

                if (request == null)
                {
                    return Json(new { success = false, message = "Lỗi parse JSON" });
                }

                if (string.IsNullOrWhiteSpace(request.ClassName))
                {
                    return Json(new { success = false, message = "Tên lớp không được để trống!" });
                }
                if (request.MajorID <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn ngành!" });
                }
                if (string.IsNullOrWhiteSpace(request.StudyTime))
                {
                    return Json(new { success = false, message = "Thời gian học không được để trống!" });
                }

                if (!DateTime.TryParse(request.StudyTime, out DateTime studyTime))
                {
                    return Json(new { success = false, message = "Thời gian học không hợp lệ!" });
                }

                var classModel = new Class
                {
                    ClassName = request.ClassName,
                    MajorID = request.MajorID,
                    StudyTime = studyTime
                };

                _context.Classes.Add(classModel);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm lớp học thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditClass(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var classModel = await _context.Classes
                .Include(c => c.Major)
                .FirstOrDefaultAsync(c => c.ClassID == id);

            if (classModel == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lớp học" });
            }

            return Json(new
            {
                success = true,
                classModel = new
                {
                    ClassID = classModel.ClassID,
                    ClassName = classModel.ClassName,
                    MajorID = classModel.MajorID,
                    StudyTime = classModel.StudyTime.ToString("yyyy-MM-dd")
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditClasses(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                if (id <= 0)
                {
                    return Json(new { success = false, message = "ID lớp học không hợp lệ" });
                }

                HttpContext.Request.EnableBuffering();
                HttpContext.Request.Body.Position = 0;

                string body;
                using (var reader = new StreamReader(HttpContext.Request.Body, Encoding.UTF8, leaveOpen: true))
                {
                    body = await reader.ReadToEndAsync();
                }
                HttpContext.Request.Body.Position = 0;

                if (string.IsNullOrWhiteSpace(body))
                {
                    return Json(new { success = false, message = "Không có dữ liệu gửi lên" });
                }

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var request = JsonSerializer.Deserialize<EditClassRequest>(body, options);

                if (request == null)
                {
                    return Json(new { success = false, message = "Lỗi parse JSON" });
                }

                if (string.IsNullOrWhiteSpace(request.ClassName))
                {
                    return Json(new { success = false, message = "Tên lớp không được để trống!" });
                }
                if (request.MajorID <= 0)
                {
                    return Json(new { success = false, message = "Vui lòng chọn ngành!" });
                }
                if (string.IsNullOrWhiteSpace(request.StudyTime))
                {
                    return Json(new { success = false, message = "Thời gian học không được để trống!" });
                }

                if (!DateTime.TryParse(request.StudyTime, out DateTime studyTime))
                {
                    return Json(new { success = false, message = "Thời gian học không hợp lệ!" });
                }

                var classModel = await _context.Classes.FindAsync(id);
                if (classModel == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy lớp học" });
                }

                classModel.ClassName = request.ClassName;
                classModel.MajorID = request.MajorID;
                classModel.StudyTime = studyTime;

                _context.Update(classModel);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật lớp học thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteClass(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var classModel = await _context.Classes
                .Include(c => c.StudentClasses)
                .Include(c => c.Timetable)
                .FirstOrDefaultAsync(c => c.ClassID == id);

            if (classModel == null)
            {
                return Json(new { success = false, message = "Không tìm thấy lớp học" });
            }

            try
            {
                // Xóa lớp học (các bản ghi liên quan sẽ tự động xóa do cascade)
                _context.Classes.Remove(classModel);
                await _context.SaveChangesAsync();
                
                return Json(new { success = true, message = "Xóa lớp học thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                return Json(new { success = false, message = $"Lỗi database: {innerEx}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ========== QUẢN LÝ MÔN HỌC ==========
        public async Task<IActionResult> Courses(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var courses = await _context.Courses
                .Include(c => c.Lecturer)
                .OrderBy(c => c.CourseName)
                .ToListAsync();

            ViewBag.Lecturers = await _context.Lecturers.ToListAsync();
            ViewData["IsPartial"] = partial;

            return partial ? PartialView(courses) : View(courses);
        }

        [HttpPost]
        public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                if (string.IsNullOrWhiteSpace(request.CourseName))
                {
                    return Json(new { success = false, message = "Tên môn học không được để trống!" });
                }
                if (request.Credits <= 0)
                {
                    return Json(new { success = false, message = "Số tín chỉ phải lớn hơn 0!" });
                }

                var course = new Course
                {
                    CourseName = request.CourseName,
                    Credits = request.Credits,
                    Description = request.Description,
                    LecturerID = request.LecturerID
                };

                _context.Courses.Add(course);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Thêm môn học thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpGet]
        public async Task<IActionResult> EditCourse(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var course = await _context.Courses.FindAsync(id);

            if (course == null)
            {
                return Json(new { success = false, message = "Không tìm thấy môn học" });
            }

            return Json(new
            {
                success = true,
                course = new
                {
                    course.CourseID,
                    course.CourseName,
                    course.Credits,
                    course.Description,
                    course.LecturerID
                }
            });
        }

        [HttpPost]
        public async Task<IActionResult> EditCourse(int id, [FromBody] EditCourseRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            if (id != request.CourseID)
            {
                return Json(new { success = false, message = "ID không khớp" });
            }

            try
            {
                var course = await _context.Courses.FindAsync(id);
                if (course == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy môn học" });
                }

                course.CourseName = request.CourseName;
                course.Credits = request.Credits;
                course.Description = request.Description;
                course.LecturerID = request.LecturerID;

                _context.Update(course);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Cập nhật môn học thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCourse(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var course = await _context.Courses.FindAsync(id);
            if (course == null)
            {
                return Json(new { success = false, message = "Không tìm thấy môn học" });
            }

            try
            {
                _context.Courses.Remove(course);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa môn học thành công!" });
            }
            catch (DbUpdateException dbEx)
            {
                var innerEx = dbEx.InnerException?.Message ?? dbEx.Message;
                return Json(new { success = false, message = $"Lỗi database: {innerEx}" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ========== QUẢN LÝ TÀI KHOẢN ==========
        public async Task<IActionResult> Accounts(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            var accounts = await _context.Accounts
                .Include(a => a.Student)
                .Include(a => a.Lecturer)
                .OrderBy(a => a.Username)
                .ToListAsync();

            ViewBag.Students = await _context.Students.ToListAsync();
            ViewBag.Lecturers = await _context.Lecturers.ToListAsync();
            ViewData["IsPartial"] = partial;

            return partial ? PartialView(accounts) : View(accounts);
        }

        [HttpPost]
        public async Task<IActionResult> CreateAccount([FromBody] CreateAccountRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            try
            {
                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(request.Username))
                {
                    return Json(new { success = false, message = "Username không được để trống!" });
                }
                if (string.IsNullOrWhiteSpace(request.Password))
                {
                    return Json(new { success = false, message = "Password không được để trống!" });
                }
                if (string.IsNullOrWhiteSpace(request.Role))
                {
                    return Json(new { success = false, message = "Role không được để trống!" });
                }

                // Kiểm tra username đã tồn tại
                var existingAccount = await _context.Accounts
                    .FirstOrDefaultAsync(a => a.Username == request.Username);

                if (existingAccount != null)
                {
                    return Json(new { success = false, message = "Username đã tồn tại!" });
                }

                // Validate role và user ID
                if (request.Role == "Student" && !request.StudentID.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng chọn sinh viên!" });
                }
                if (request.Role == "Lecturer" && !request.LecturerID.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng chọn giảng viên!" });
                }

                // Hash password
                var passwordHash = HashPassword(request.Password);

                var account = new Account
                {
                    Username = request.Username,
                    PasswordHash = passwordHash,
                    Role = request.Role,
                    StudentID = request.StudentID,
                    LecturerID = request.LecturerID
                };

                _context.Accounts.Add(account);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Tạo tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> DeleteAccount(int id)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Không có quyền truy cập" });
            }

            var account = await _context.Accounts.FindAsync(id);
            if (account == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản" });
            }

            try
            {
                _context.Accounts.Remove(account);
                await _context.SaveChangesAsync();
                return Json(new { success = true, message = "Xóa tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // ========== THỐNG KÊ & BÁO CÁO ==========
        public async Task<IActionResult> Statistics(bool partial = false)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            // Thống kê chi tiết
            var totalStudents = await _context.Students.CountAsync();
            var totalLecturers = await _context.Lecturers.CountAsync();
            var totalCourses = await _context.Courses.CountAsync();
            var totalClasses = await _context.Classes.CountAsync();

            // Thống kê điểm danh
            var totalAttendance = await _context.Attendances.CountAsync();
            var presentCount = await _context.Attendances.CountAsync(a => a.Status == "Có mặt");
            var attendanceRate = totalAttendance > 0 ? Math.Round((double)presentCount / totalAttendance * 100, 2) : 0;

            // Thống kê điểm số
            var enrollments = await _context.Enrollments
                .Where(e => e.Score.HasValue)
                .ToListAsync();
            var avgScore = enrollments.Any() ? Math.Round(enrollments.Average(e => e.Score!.Value), 2) : 0;

            // Thống kê theo khoa
            var departments = await _context.Departments
                .Include(d => d.Lecturers)
                .Select(d => new
                {
                    DepartmentName = d.DepartmentName,
                    LecturerCount = d.Lecturers.Count
                })
                .ToListAsync();

            ViewBag.TotalStudents = totalStudents;
            ViewBag.TotalLecturers = totalLecturers;
            ViewBag.TotalCourses = totalCourses;
            ViewBag.TotalClasses = totalClasses;
            ViewBag.TotalAttendance = totalAttendance;
            ViewBag.AttendanceRate = attendanceRate;
            ViewBag.AvgScore = avgScore;
            ViewBag.Departments = departments;

            ViewData["IsPartial"] = partial;

            return partial ? PartialView() : View();
        }

        // Helper method để hash password
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }
    }

    // Request models
    public class CreateStudentRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string DOB { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ClassID { get; set; }
    }

    public class EditStudentRequest
    {
        public int StudentId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public string DOB { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public int? ClassID { get; set; }
    }

    public class CreateLecturerRequest
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? DepartmentID { get; set; }
    }

    public class EditLecturerRequest
    {
        public int LecturerID { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Phone { get; set; }
        public int? DepartmentID { get; set; }
    }

    public class CreateCourseRequest
    {
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int? LecturerID { get; set; }
    }

    public class EditCourseRequest
    {
        public int CourseID { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string? Description { get; set; }
        public int? LecturerID { get; set; }
    }

    public class CreateClassRequest
    {
        public string ClassName { get; set; } = string.Empty;
        public int MajorID { get; set; }
        public string StudyTime { get; set; } = string.Empty;
    }

    public class EditClassRequest
    {
        public int ClassID { get; set; }
        public string ClassName { get; set; } = string.Empty;
        public int MajorID { get; set; }
        public string StudyTime { get; set; } = string.Empty;
    }

    public class CreateAccountRequest
    {
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public int? StudentID { get; set; }
        public int? LecturerID { get; set; }
    }
}
