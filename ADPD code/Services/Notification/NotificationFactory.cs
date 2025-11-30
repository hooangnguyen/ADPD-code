using System;
using ADPD_code.Data;
using ADPD_code.Models;
using Microsoft.Extensions.Configuration;

namespace ADPD_code.Services.Notification
{
    /// <summary>
    /// Factory Pattern - Tạo các notification services
    /// </summary>
    public class NotificationFactory : INotificationFactory
    {
        private readonly IConfiguration _configuration;
        private readonly ApplicationDbContext _context;

        public NotificationFactory(IConfiguration configuration, ApplicationDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }

        /// <summary>
        /// Tạo notification service dựa trên loại
        /// </summary>
        public INotificationService CreateNotificationService(NotificationType type)
        {
            return type switch
            {
                NotificationType.Email => new EmailNotificationService(_configuration, _context),
                NotificationType.SMS => new SMSNotificationService(_configuration, _context),
                NotificationType.InApp => new InAppNotificationService(_context),
                NotificationType.Push => new PushNotificationService(_configuration, _context),
                _ => throw new ArgumentException($"Loại thông báo không hợp lệ: {type}")
            };
        }
    }
}