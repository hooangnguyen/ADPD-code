using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Course")]
    public class Course
    {
        [Key]
        public int CourseID { get; set; }

        [Required, StringLength(100)]
        public string? CourseName { get; set; }

        [Required]
        public int Credits { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        // LecturerID can be nullable to support OnDelete(SetNull)
        [ForeignKey(nameof(Lecturer))]
        public int? LecturerID { get; set; }
        public Lecturer? Lecturer { get; set; }

        // Navigation collections used by DbContext
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
        public ICollection<Timetable> Timetable { get; set; } = new List<Timetable>();
        public ICollection<AssignmentAttachment> AssignmentAttachments { get; set; } = new List<AssignmentAttachment>();
    }
}
