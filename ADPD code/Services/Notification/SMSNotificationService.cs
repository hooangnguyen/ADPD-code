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
    /// Service gửi SMS (tạm thời giả lập)
    /// </summary>
    public class SMSNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public SMSNotificationService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public NotificationType GetNotificationType()
        {
            return NotificationType.SMS;
        }

        public async Task<bool> SendAsync(NotificationModel notification)
        {
            try
            {
                if (string.IsNullOrEmpty(notification.RecipientPhone))
                {
                    throw new ArgumentException("Số điện thoại không được để trống");
                }

                // Hiện tại giả lập gửi SMS
                // Sau này có thể tích hợp Twilio hoặc dịch vụ SMS khác
                System.Diagnostics.Debug.WriteLine(
                    $"[SMS] Gửi đến {notification.RecipientPhone}: {notification.Message}");

                notification.Status = NotificationStatus.Delivered;
                notification.SentDate = DateTime.Now;

                _context.Update(notification);
                await _context.SaveChangesAsync();

                await LogNotificationAsync(notification, "SMS gửi thành công (giả lập)");

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
                Action = "SMS Send",
                Details = action
            };

            _context.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}