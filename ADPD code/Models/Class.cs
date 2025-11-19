using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Classes")]
    public class Class
    {
       [Key]  
        public int ClassID { get; set; }
        [Required]  
        [DataType(DataType.Text)]
        public string ClassName { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string StudyTime { get; set; }
    }
}
