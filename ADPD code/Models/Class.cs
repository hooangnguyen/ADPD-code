using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Cần thêm namespace này

namespace ADPD_code.Models
{
    [Table("Class")] // Đã sửa lại tên bảng Class (trong SQL bạn đặt là Class)
    public class Class
    {
        [Key]
        public int ClassID { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public string ClassName { get; set; }

        [Required]
        [DataType(DataType.Text)]
        public DateTime StudyTime { get; set; }
        [ForeignKey(nameof(Major))]
        public int MajorID { get; set; } // Khóa ngoại trỏ tới bảng Major
        public Major Major { get; set; } // Navigation property

        // --- THAY ĐỔI CHO QUAN HỆ N:M ---

        // Thuộc tính điều hướng đến bảng trung gian StudentClass
        public ICollection<StudentClass> StudentClasses { get; set; }
    }
}