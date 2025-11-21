using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Major")] // Lưu ý: Tên bảng SQL ban đầu là Major, không phải Majors
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

        // Cần thêm thuộc tính điều hướng ngược nếu muốn xem Class nào thuộc Major này
        public ICollection<Class> Classes { get; set; } // Thêm nếu cần quan hệ 1:N với Class
    }
}