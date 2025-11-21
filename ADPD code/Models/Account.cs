using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Account")]
    public class Account
    {
        [Key]
        public int UserID { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string? Username { get; set; }
        [Required]
        [DataType(DataType.Text)]
        public string? PasswordHash { get; set; }
        public string? Role { get; set; } // -- Admin / Student / Lecturer   
        [ForeignKey(nameof(Student))]
        public int StudentID { get; set; }
        public Student? Student { get; set; }
        [ForeignKey(nameof(Lecturer))]
        public int LecturerID { get;set; }
        public Lecturer? Lecturer { get; set; }
    }
}
