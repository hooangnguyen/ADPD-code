using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Assignment")]
    public class Assignment
    {
        [Key]
        public int AssignmentID { get; set; }
        [ForeignKey(nameof(Course))]
        public int CourseID { get; set; }
        public Course Course { get; set; }
        [ForeignKey(nameof(Lecturer))]
        public int LecturerID { get; set; }
        public Lecturer lecturer { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Title { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string Description { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

    }
}
