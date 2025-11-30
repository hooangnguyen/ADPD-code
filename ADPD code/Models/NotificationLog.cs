using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("NotificationLog")]
    public class NotificationLog
    {
        [Key]
        public int LogID { get; set; }

        [Required]
        public int NotificationID { get; set; }

        [ForeignKey(nameof(NotificationID))]
        public Notification Notification { get; set; }

        public DateTime LogDate { get; set; }

        public string Action { get; set; }

        public string Details { get; set; }
    }
}