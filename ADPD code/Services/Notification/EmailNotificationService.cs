using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using ADPD_code.Data;
using ADPD_code.Models;
using Microsoft.Extensions.Configuration;
using NotificationModel = ADPD_code.Models.Notification;
using NotificationLogModel = ADPD_code.Models.NotificationLog;

namespace ADPD_code.Services.Notification
{
    public class EmailNotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public EmailNotificationService(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        public NotificationType GetNotificationType()
        {
            return NotificationType.Email;
        }

        public async Task<bool> SendAsync(NotificationModel notification)
        {
            try
            {
                if (string.IsNullOrEmpty(notification.RecipientEmail))
                {
                    throw new ArgumentException("Email không được để trống");
                }

                // Lấy cấu hình SMTP từ appsettings
                var smtpServer = _configuration["Email:SmtpServer"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");
                var senderEmail = _configuration["Email:SenderEmail"];
                var senderPassword = _configuration["Email:SenderPassword"];

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = true;
                    client.Credentials = new NetworkCredential(senderEmail, senderPassword);

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(senderEmail, "StudentMS"),
                        Subject = notification.Title,
                        Body = FormatEmailBody(notification.Message),
                        IsBodyHtml = true
                    };

                    mailMessage.To.Add(notification.RecipientEmail);
                    await client.SendMailAsync(mailMessage);

                    // Cập nhật trạng thái
                    notification.Status = NotificationStatus.Delivered;
                    notification.SentDate = DateTime.Now;

                    _context.Update(notification);
                    await _context.SaveChangesAsync();

                    // Ghi log
                    await LogNotificationAsync(notification, "Email gửi thành công");

                    return true;
                }
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

        private string FormatEmailBody(string message)
        {
            return $@"
                <html>
                <head>
                    <style>
                        body {{ font-family: Arial, sans-serif; color: #333; }}
                        .container {{ max-width: 600px; margin: 0 auto; }}
                        .header {{ background-color: #667eea; color: white; padding: 20px; text-align: center; }}
                        .content {{ padding: 20px; background-color: #f9f9f9; }}
                        .footer {{ background-color: #f0f0f0; padding: 10px; text-align: center; font-size: 12px; }}
                    </style>
                </head>
                <body>
                    <div class='container'>
                        <div class='header'>
                            <h2>📧 Thông báo từ StudentMS</h2>
                        </div>
                        <div class='content'>
                            {message}
                        </div>
                        <div class='footer'>
                            <p>© 2024 StudentMS - Hệ thống Quản lý Sinh viên</p>
                            <p>Vui lòng không trả lời email này</p>
                        </div>
                    </div>
                </body>
                </html>";
        }

        private async Task LogNotificationAsync(NotificationModel notification, string action)
        {
            var log = new NotificationLogModel
            {
                NotificationID = notification.NotificationID,
                LogDate = DateTime.Now,
                Action = "Email Send",
                Details = action
            };

            _context.Add(log);
            await _context.SaveChangesAsync();
        }
    }
}