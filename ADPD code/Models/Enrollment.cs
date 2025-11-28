using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Enrollment")]
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentID { get; set; }

        [ForeignKey(nameof(Course))]
        public int CourseID { get; set; }

        [StringLength(20)]
        public string? Semester { get; set; }

        [StringLength(20)]
        public string? AcademicYear { get; set; }

        public double? Score { get; set; }

        public Student? Student { get; set; }
        public Course? Course { get; set; }
    }
}
