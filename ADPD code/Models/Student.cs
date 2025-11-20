using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Cần thêm namespace này

namespace ADPD_code.Models
{
    [Table("Student")]
    public class Student
    {
        [Key]
        public int StudentId { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string FullName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Gender { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime DOB { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Phone { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Address { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string Status { get; set; }

        // --- THAY ĐỔI CHO QUAN HỆ N:M ---

        // Thuộc tính điều hướng đến bảng trung gian StudentClass
        public ICollection<StudentClass> StudentClasses { get; set; }
    }
}