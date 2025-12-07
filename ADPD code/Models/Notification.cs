using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    // Enum định nghĩa loại thông báo
    public enum NotificationType
    {
        Email,      // Gửi email
        SMS,        // Gửi tin nhắn SMS
        InApp,      // Thông báo trong ứng dụng
        Push        // Gửi push notification
    }

    // Enum định nghĩa trạng thái
    public enum NotificationStatus
    {
        Pending,    // Chờ gửi
        Delivered,  // Đã gửi thành công
        Failed      // Thất bại
    }

    [Table("Notification")]
    public class Notification
    {
        [Key]
        public int NotificationID { get; set; }

        [Required]
        public int RecipientID { get; set; }

        [Required, StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Message { get; set; } = string.Empty;

        [Required]
        public NotificationType Type { get; set; }

        [Required]
        public NotificationStatus Status { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? SentDate { get; set; }

        [StringLength(255)]
        public string? RecipientEmail { get; set; }

        [StringLength(20)]
        public string? RecipientPhone { get; set; }

        public string? ErrorMessage { get; set; }

        [StringLength(50)]
        public string? Priority { get; set; } // High, Medium, Low

        // NEW: Track read/unread for in-app notifications
        public bool IsRead { get; set; } = false;
    }
}