using System.Threading.Tasks;
using ADPD_code.ViewModels;

namespace ADPD_code.Services
{
    public interface ILecturerAnalyticsService
    {
        Task<LecturerDashboardStats> GetDashboardStats(int lecturerId);
    }

    public class LecturerDashboardStats
    {
        public int TotalCourses { get; set; }
        public int TotalStudents { get; set; }
        public int PendingAssignments { get; set; }
        public int TodayClasses { get; set; }
    }
}
