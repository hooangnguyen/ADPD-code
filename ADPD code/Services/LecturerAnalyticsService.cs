using System;
using System.Linq;
using System.Threading.Tasks;
using ADPD_code.Data;
using ADPD_code.Services;
using Microsoft.EntityFrameworkCore;

namespace ADPD_code.Services
{
    public class LecturerAnalyticsService : ILecturerAnalyticsService
    {
        private readonly IDbContextFactory<ApplicationDbContext> _contextFactory;

        public LecturerAnalyticsService(IDbContextFactory<ApplicationDbContext> contextFactory)
        {
            _contextFactory = contextFactory;
        }

        public async Task<LecturerDashboardStats> GetDashboardStats(int lecturerId)
        {
            using (var context = _contextFactory.CreateDbContext())
            {
                var totalCourses = await context.Courses
                    .Where(c => c.LecturerID == lecturerId)
                    .CountAsync();

                var totalStudents = await context.Enrollments
                    .Include(e => e.Course)
                    .Where(e => e.Course.LecturerID == lecturerId)
                    .Select(e => e.StudentID)
                    .Distinct()
                    .CountAsync();

                var pendingAssignments = await context.AssignmentSubmissions
                    .Include(s => s.Assignment)
                    .Where(s => s.Assignment.LecturerID == lecturerId && (!s.Score.HasValue || s.Score == 0))
                    .CountAsync();
                
                var today = DateTime.Today;
                var todayClasses = await context.Timetable
                    .Include(t => t.Course)
                    .Where(t => t.Course.LecturerID == lecturerId && t.StudyDate == today)
                    .CountAsync();

                return new LecturerDashboardStats
                {
                    TotalCourses = totalCourses,
                    TotalStudents = totalStudents,
                    PendingAssignments = pendingAssignments,
                    TodayClasses = todayClasses
                };
            }
        }
    }
}
