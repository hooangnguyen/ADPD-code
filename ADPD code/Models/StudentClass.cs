using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ADPD_code.Models
{
    [Table("StudentClass")]
    public class StudentClass
    {
        [Key]
        public int StudentClassID { get; set; }

        [ForeignKey(nameof(Student))]
        public int StudentId { get; set; }
        public Student? Student { get; set; }

        [ForeignKey(nameof(Class))]
        public int ClassID { get; set; }
        public Class? Class { get; set; }
    }
}
