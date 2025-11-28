using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    // 1. Sửa tên bảng thành số ít cho khớp SQL
    [Table("Department")]
    public class Department
    {
        [Key]
        public int DepartmentID { get; set; }

        [Required, StringLength(100)]
        public string? DepartmentName { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        // Navigation
        public ICollection<Lecturer> Lecturers { get; set; } = new List<Lecturer>();
    }
}