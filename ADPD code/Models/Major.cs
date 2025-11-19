using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Majors")]
    public class Major
    {
        [Key]
        public int MajorID { get; set; }    
        [Required]
        [DataType(DataType.Text)]   
        public string MajorName { get; set; }   
        [Required]
        [DataType(DataType.Text)]
        public string Description { get; set; }

    }
}
