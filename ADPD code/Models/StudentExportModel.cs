namespace ADPD_code.Models
{
    public class StudentExportModel
    {
        public int Index { get; set; }
        public string MSSV { get; set; } = "";
        public string FullName { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public double GPA { get; set; }
        public double AttendanceRate { get; set; }
        public string Status { get; set; } = "";
    }


}
