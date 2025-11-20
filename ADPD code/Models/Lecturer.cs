using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Lecturer")]
    public class Lecturer
    {
        [Key]
        public int LecturerID { get; set; } // Sửa từ Id thành LecturerID

        [Required]
        [DataType(DataType.Text)]
        public string FullName { get; set; } // Sửa từ Name thành FullName

        [Required]
        [DataType(DataType.EmailAddress)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Phone { get; set; } // Bổ sung cột Phone

        // --- QUAN HỆ KHÓA NGOẠI VỚI DEPARTMENT ---

        [ForeignKey(nameof(Department))]
        public int DepartmentID { get; set; } // Khóa ngoại

        // Thuộc tính điều hướng (Navigation Property)
        public Department Department { get; set; }
    }
}