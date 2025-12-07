// ADPD code/Controllers/AdminNotificationController.cs
using ADPD_code.Data;
using ADPD_code.Models;
using ADPD_code.Services.Notification;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ADPD_code.Controllers
{
    public class AdminNotificationController : Controller
    {
        private readonly INotificationManager _notificationManager;
        private readonly ApplicationDbContext _context;

        public AdminNotificationController(
            INotificationManager notificationManager,
            ApplicationDbContext context)
        {
            _notificationManager = notificationManager;
            _context = context;
        }

        // GET: Admin/Notifications
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            // Lấy danh sách thông báo gần đây
            var notifications = await _context.Notifications
                .OrderByDescending(n => n.CreatedDate)
                .Take(50)
                .ToListAsync();

            return View(notifications);
        }

        // GET: Admin/Notifications/Create
        public IActionResult Create()
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return RedirectToAction("Index", "Login");
            }

            return View();
        }

        // POST: Admin/Notifications/Send
        [HttpPost]
        public async Task<IActionResult> Send([FromBody] SendNotificationRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                // 🎯 ĐÂY LÀ  FACTORY PATTERN
                // NotificationManager sẽ dùng Factory để tạo đúng service
                var result = await _notificationManager.SendNotificationAsync(
                    recipientId: request.RecipientId,
                    title: request.Title,
                    message: request.Message,
                    type: request.Type,
                    email: request.Email,
                    phone: request.Phone,
                    priority: request.Priority
                );

                return Json(new
                {
                    success = result,
                    message = result ? "Gửi thông báo thành công!" : "Gửi thất bại"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Admin/Notifications/SendToAll
        [HttpPost]
        public async Task<IActionResult> SendToAll([FromBody] BroadcastRequest request)
        {
            if (HttpContext.Session.GetString("Role") != "Admin")
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            try
            {
                // Lấy tất cả sinh viên
                var students = await _context.Students.ToListAsync();
                int successCount = 0;

                foreach (var student in students)
                {
                    var result = await _notificationManager.SendNotificationAsync(
                        recipientId: student.StudentId,
                        title: request.Title,
                        message: request.Message,
                        type: request.Type,
                        email: student.Email,
                        phone: student.Phone,
                        priority: request.Priority
                    );

                    if (result) successCount++;
                }

                return Json(new
                {
                    success = true,
                    message = $"Đã gửi {successCount}/{students.Count} thông báo"
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }

    // Request models
    public class SendNotificationRequest
    {
        public int RecipientId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Priority { get; set; } = "Medium";
    }

    public class BroadcastRequest
    {
        public string Title { get; set; }
        public string Message { get; set; }
        public NotificationType Type { get; set; }
        public string Priority { get; set; } = "Medium";
    }
}