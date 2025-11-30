using System.Threading.Tasks;
using ADPD_code.Models;

namespace ADPD_code.Services.Notification
{
    /// <summary>
    /// Manager interface để quản lý gửi thông báo
    /// </summary>
    public interface INotificationManager
    {
        /// <summary>
        /// Gửi thông báo cho một người dùng
        /// </summary>
        Task<bool> SendNotificationAsync(
            int recipientId,
            string title,
            string message,
            NotificationType type,
            string email = null,
            string phone = null,
            string priority = "Medium");

        /// <summary>
        /// Gửi thông báo cho tất cả người dùng
        /// </summary>
        Task<bool> SendBroadcastAsync(
            string title,
            string message,
            NotificationType type,
            string priority = "Medium");
    }
}