using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("Account")]
    public class Account
    {
        [Key]
        public int UserID { get; set; }

        [Required, StringLength(50)]
        public string? Username { get; set; }

        [Required, StringLength(255)]
        public string? PasswordHash { get; set; }

        [StringLength(20)]
        public string? Role { get; set; }

        public int? StudentID { get; set; }
        public Student? Student { get; set; }

        public int? LecturerID { get; set; }
        public Lecturer? Lecturer { get; set; }
    }
}
