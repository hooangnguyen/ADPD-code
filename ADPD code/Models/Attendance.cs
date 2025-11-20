using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Attendances")]
    public class Attendance
    {
        [Key]
        public int AttendanceID { get; set; }
        [ForeignKey(nameof(Student))]
        public int StudentID { get; set; }
        public Student Student { get; set; }
        [ForeignKey(nameof(Course))]
        public int CourseID { get; set; }
        public Course Course { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }  
        [Required]
        [DataType(DataType.Text)]
        public string Status { get; set; }



    }
}
