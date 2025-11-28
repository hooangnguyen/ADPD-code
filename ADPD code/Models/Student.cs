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
        public string? FullName { get; set; }

        [Required]
        public string? Gender { get; set; }

        [Required]
        public DateTime DOB { get; set; }

        [Required]
        public string? Email { get; set; }

        [Required]
        public string? Phone { get; set; }

        [Required]
        public string? Address { get; set; }

        [Required]
        public string? Status { get; set; }

        // Navigation collections used by ApplicationDbContext mapping
        public ICollection<StudentClass> StudentClasses { get; set; } = new List<StudentClass>();
        public ICollection<Enrollment> Enrollments { get; set; } = new List<Enrollment>();
        public ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public ICollection<AssignmentSubmission> AssignmentSubmissions { get; set; } = new List<AssignmentSubmission>();
    }
}