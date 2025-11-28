using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Attendance")]
    public class Attendance
    {
        [Key]
        public int AttendanceID { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentID { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseID { get; set; }

        public DateTime Date { get; set; }

        [StringLength(20)]
        public string? Status { get; set; }

        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }
}
