using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Enrollments")]
    public class Enrollment
    {
        [Key]
        public int EnrollmentID { get; set; }
        [ForeignKey(nameof(Student))]
        public int StudentID { get; set; }
        public Student Student { get; set; }
        [ForeignKey(nameof(Course))]
        public int CourseID { get; set; }
        public Course Course { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Semester { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string AcademicYear { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Score { get; set; }
    }
}
