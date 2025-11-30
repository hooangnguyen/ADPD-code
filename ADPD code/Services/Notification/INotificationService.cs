using System.Threading.Tasks;
using ADPD_code.Models;
using NotificationModel = ADPD_code.Models.Notification;

namespace ADPD_code.Services.Notification
{
    /// <summary>
    /// Interface cho tất cả notification services
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Gửi thông báo
        /// </summary>
        Task<bool> SendAsync(NotificationModel notification);

        /// <summary>
        /// Lấy loại thông báo
        /// </summary>
        NotificationType GetNotificationType();
    }
}