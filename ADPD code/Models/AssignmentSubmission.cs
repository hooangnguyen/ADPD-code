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
        public Assignment Assignment{ get; set; }
        [ForeignKey(nameof(Student))]
        public int StudentID { get; set; }
        public DateTime SubmitDate { get; set; }
        public string FilePath { get; set; }
        public string AnswerText{ get; set ;}
        public float Score { get; set; }
        public string Feedback {get ; set; }
    }
}
