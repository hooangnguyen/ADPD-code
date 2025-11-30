using System;
using System.Threading.Tasks;
using ADPD_code.Data;
using ADPD_code.Models;

namespace ADPD_code.Services.Notification
{
    /// <summary>
    /// Manager quản lý gửi thông báo
    /// </summary>
    public class NotificationManager : INotificationManager
    {
        private readonly INotificationFactory _factory;
        private readonly ApplicationDbContext _context;

        public NotificationManager(INotificationFactory factory, ApplicationDbContext context)
        {
            _factory = factory;
            _context = context;
        }

        public async Task<bool> SendNotificationAsync(
            int recipientId,
            string title,
            string message,
            NotificationType type,
            string email = null,
            string phone = null,
            string priority = "Medium")
        {
            try
            {
                // Tạo notification object
                var notification = new ADPD_code.Models.Notification
                {
                    RecipientID = recipientId,
                    Title = title,
                    Message = message,
                    Type = type,
                    Status = NotificationStatus.Pending,
                    CreatedDate = DateTime.Now,
                    RecipientEmail = email,
                    RecipientPhone = phone,
                    Priority = priority
                };

                // Lưu vào database
                _context.Add(notification);
                await _context.SaveChangesAsync();

                // Tạo service từ factory
                var service = _factory.CreateNotificationService(type);

                // Gửi thông báo
                return await service.SendAsync(notification);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Lỗi: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendBroadcastAsync(
            string title,
            string message,
            NotificationType type,
            string priority = "Medium")
        {
            // TODO: Implement logic gửi cho tất cả
            return await Task.FromResult(true);
        }
    }
}