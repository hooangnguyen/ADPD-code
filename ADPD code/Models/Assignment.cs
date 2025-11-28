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
        public Course? Course { get; set; }

        [ForeignKey(nameof(Lecturer))]
        public int LecturerID { get; set; }
        public Lecturer? Lecturer { get; set; }

        [Required, StringLength(200)]
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        // Navigation
        public ICollection<AssignmentAttachment> AssignmentAttachments { get; set; } = new List<AssignmentAttachment>();
        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    }
}
