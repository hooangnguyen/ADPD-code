using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic; // Cần thêm namespace này

namespace ADPD_code.Models
{
    [Table("Class")]
    public class Class
    {
        [Key]
        public int ClassID { get; set; }

        [Required, StringLength(100)]
        public string? ClassName { get; set; }

        [Required]
        public DateTime StudyTime { get; set; }

        [Required]
        public int MajorID { get; set; }

        // Navigation
        public Major? Major { get; set; }
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public ICollection<Timetable>? Timetable { get; set; } 
    }
}
