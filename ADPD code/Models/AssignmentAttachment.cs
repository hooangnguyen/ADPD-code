using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("AssignmentAttachment")]
    public class AssignmentAttachment
    {
        [Key]
        public string AttachmentID { get; set; }
        [ForeignKey(nameof(Attendance))]
        public Attendance Attendance { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string FilePath { get; set; }

    }
}
