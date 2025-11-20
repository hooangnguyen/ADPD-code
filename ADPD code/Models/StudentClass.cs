using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("StudentClass")]
    public class StudentClass
    {
        // Khóa chính có thể là khóa ghép (Composite Key) 
        // hoặc một khóa đơn (như StudentClassID)
        [Key]
        public int StudentClassID { get; set; }

        // Khóa ngoại 1
        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }
        public Student Student { get; set; } // Thuộc tính điều hướng

        // Khóa ngoại 2
        [ForeignKey(nameof(Class))]
        public int ClassID { get; set; }
        public Class Class { get; set; } // Thuộc tính điều hướng
    }
}