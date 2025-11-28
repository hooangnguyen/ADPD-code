using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Major")]
    public class Major
    {
        [Key]
        public int MajorID { get; set; }

        [Required, StringLength(100)]
        public string? MajorName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }
        public ICollection<Class> Classes { get; set; } = new List<Class>();
    }
}