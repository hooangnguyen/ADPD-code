using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("AssignmentSubmission")]
    public class AssignmentSubmission
    {
        [Key]
        public int SubmissionID { get; set; }

        [ForeignKey(nameof(Assignment))]
        public int AssignmentID { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentID { get; set; }

        public DateTime SubmitDate { get; set; }

        [StringLength(500)]
        public string? FilePath { get; set; }

        public string? AnswerText { get; set; }
        public double? Score { get; set; }
        public string? Feedback { get; set; }

        public Assignment? Assignment { get; set; }
        public Student? Student { get; set; }
    }
}
