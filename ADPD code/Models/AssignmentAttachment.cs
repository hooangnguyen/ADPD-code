using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("AssignmentAttachment")]
    public class AssignmentAttachment
    {
        [Key]
        public int AttachmentID { get; set; }

        [ForeignKey(nameof(Assignment))]
        public int AssignmentID { get; set; }

        [Required, StringLength(500)]
        public string? FilePath { get; set; }
        public Assignment? Assignment { get; set; }
    }
}
