namespace ADPD_code.ViewModels
{
    public class StudentSubjectViewModel
    {
        public int CourseId { get; set; }
        public string CourseName { get; set; } = string.Empty;
        public string CourseCode { get; set; } = string.Empty;
        public int Credits { get; set; }
        public string LecturerName { get; set; } = string.Empty;
        public string Semester { get; set; } = string.Empty;
        public string AcademicYear { get; set; } = string.Empty;
        public double? Score { get; set; }
        public string ClassName { get; set; } = string.Empty;
    }
}

