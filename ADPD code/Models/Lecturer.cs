using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Lecturer")]
    public class Lecturer
    {
        [Key]
        public int LecturerID { get; set; }

        [Required]
        public string? FullName { get; set; }

        [Required, DataType(DataType.EmailAddress)]
        public string? Email { get; set; }

        [StringLength(20)]
        public string? Phone { get; set; }

        [ForeignKey(nameof(Department))]
        public int DepartmentID { get; set; }

        public Department? Department { get; set; }

        // Navigation collections referenced by DbContext
        public ICollection<Course> Courses { get; set; } = new List<Course>();
        public ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();
    }
}