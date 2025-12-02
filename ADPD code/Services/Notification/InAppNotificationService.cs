using System;
using System.Threading.Tasks;
using ADPD_code.Data;
using ADPD_code.Models;
using NotificationModel = ADPD_code.Models.Notification;
using NotificationLogModel = ADPD_code.Models.NotificationLog;

namespace ADPD_code.Services.Notification
{
    /// <summary>
    /// Service gửi thông báo trong ứng dụng
    /// </summary>
    public class InAppNotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public InAppNotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public NotificationType GetNotificationType()
        {
            return NotificationType.InApp;
        }

        public async Task<bool> SendAsync(NotificationModel notification)
        {
            try
            {
                // Không Add lại, chỉ Update vì đã được Add ở NotificationManager
                notification.Status = NotificationStatus.Delivered;
                notification.SentDate = DateTime.Now;

                _context.Update(notification);
                await _context.SaveChangesAsync();

                await LogNotificationAsync(notification, "Thông báo in-app được tạo");

                return true;
            }
            catch (Exception ex)
            {
                notification.Status = NotificationStatus.Failed;
                notification.ErrorMessage = ex.Message;

                _context.Update(notification);
                await _context.SaveChangesAsync();

                await LogNotificationAsync(notification, $"Thất bại: {ex.Message}");
                return false;
            }
        }

        private async Task LogNotificationAsync(NotificationModel notification, string action)
        {
            var log = new NotificationLogModel
            {
                NotificationID = notification.NotificationID,
                LogDate = DateTime.Now,
                Action = "InApp Create",
                Details = action
            };

            _context.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}