using System;
using System.Threading.Tasks;
using ADPD_code.Data;
using ADPD_code.Models;
using Microsoft.Extensions.Configuration;
using NotificationModel = ADPD_code.Models.Notification;
using NotificationLogModel = ADPD_code.Models.NotificationLog;

namespace ADPD_code.Services.Notification
{
    /// <summary>
    /// Service gửi push notification (tạm thời giả lập)
    /// </summary>
    public class PushNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public PushNotificationService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public NotificationType GetNotificationType()
        {
            return NotificationType.Push;
        }

        public async Task<bool> SendAsync(NotificationModel notification)
        {
            try
            {
                // Giả lập gửi push notification
                System.Diagnostics.Debug.WriteLine(
                    $"[PUSH] {notification.Title}: {notification.Message}");

                notification.Status = NotificationStatus.Delivered;
                notification.SentDate = DateTime.Now;

                _context.Update(notification);
                await _context.SaveChangesAsync();

                await LogNotificationAsync(notification, "Push notification gửi thành công (giả lập)");

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
                Action = "Push Send",
                Details = action
            };

            _context.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}