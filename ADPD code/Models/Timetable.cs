using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Timetable")]
    public class Timetable
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int TimetableID { get; set; }
        // Khóa ngoại đến Class
        public int ClassID { get; set; }
        // Khóa ngoại đến Course
        public int CourseID { get; set; }
        [Required]
        public DateTime StudyDate { get; set; } // Kiểu DATE trong SQL được ánh xạ thành DateTime trong C#
        [Required]
        public TimeSpan StartTime { get; set; } // Kiểu TIME trong SQL được ánh xạ thành TimeSpan

        [Required]
        public TimeSpan EndTime { get; set; } // Kiểu TIME trong SQL được ánh xạ thành TimeSpan
        [MaxLength(100)]
        public string? Room { get; set; }
        [ForeignKey("ClassID")]
        public virtual Class? Class { get; set; } // Liên kết đến đối tượng Class
        [ForeignKey("CourseID")]
        public virtual Course? Course { get; set; } // Liên kết đến đối tượng Course
    }
}
