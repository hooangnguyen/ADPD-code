using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Departments")]
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string DepartmentName { get; set; }  
        [Required]
        [DataType(DataType.Text)]
        public string Description{ get; set;}
    }
}
