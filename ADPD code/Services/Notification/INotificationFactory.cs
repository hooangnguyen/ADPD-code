using ADPD_code.Models;

namespace ADPD_code.Services.Notification
{
    /// <summary>
    /// Factory interface để tạo notification services
    /// </summary>
    public interface INotificationFactory
    {
        /// <summary>
        /// Tạo notification service dựa trên loại
        /// </summary>
        INotificationService CreateNotificationService(NotificationType type);
    }
}