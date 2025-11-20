using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Courses")]
    public class Course
    {
        [Key]
        public int CourseID { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string CourseName { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Credits { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Description { get; set; }
        [ForeignKey(nameof(Lecturer))]
        public int LecturerID { get; set; }
        public Lecturer Lecturer { get; set; }
    }
}
