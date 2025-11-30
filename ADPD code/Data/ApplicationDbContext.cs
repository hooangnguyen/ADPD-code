using Microsoft.EntityFrameworkCore;
using ADPD_code.Models;

namespace ADPD_code.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Timetable> Timetable { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<NotificationLog> NotificationLogs { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<AssignmentSubmission> AssignmentSubmissions { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Account> Accounts { get; set; }
        public DbSet<Enrollment> Enrollments { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Class> Classes { get; set; }
        public DbSet<Major> Majors { get; set; }
        public DbSet<StudentClass> StudentClasses { get; set; }
        public DbSet<AssignmentAttachment> AssignmentAttachments { get; set; }

        // override OnModelCreating if you need custom mapping
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ========== STUDENT ==========
            modelBuilder.Entity<Student>(e =>
            {
                e.ToTable("Student");
                e.HasKey(s => s.StudentId);
                e.Property(s => s.FullName).HasColumnName("Fullname").HasMaxLength(255).IsRequired();
                e.Property(s => s.Gender).HasColumnName("Gender").HasMaxLength(50).IsRequired();
                e.Property(s => s.DOB).HasColumnName("DOB").IsRequired();
                e.Property(s => s.Email).HasColumnName("Email").HasMaxLength(255).IsRequired();
                e.Property(s => s.Phone).HasColumnName("Phone").HasMaxLength(50).IsRequired();
                e.Property(s => s.Address).HasColumnName("Address").HasMaxLength(500).IsRequired();
                e.Property(s => s.Status).HasColumnName("Status").HasMaxLength(50).IsRequired();
            });

            // ========== STUDENTCLASS (Bảng trung gian) ==========
            modelBuilder.Entity<StudentClass>(entity =>
            {
                entity.ToTable("StudentClass");
                entity.HasKey(sc => sc.StudentClassID);

                // Quan hệ: StudentClass -> Student
                entity.HasOne(sc => sc.Student)
                      .WithMany(s => s.StudentClasses)
                      .HasForeignKey(sc => sc.StudentId)
                      .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ: StudentClass -> Class
                entity.HasOne(sc => sc.Class)
                      .WithMany(c => c.StudentClasses)
                      .HasForeignKey(sc => sc.ClassID)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== CLASS ==========
            modelBuilder.Entity<Class>(c =>
            {
                c.ToTable("Class");
                c.HasKey(x => x.ClassID);
                c.Property(x => x.ClassName).HasColumnName("ClassName").HasMaxLength(100).IsRequired();
                c.Property(x => x.StudyTime).HasColumnName("StudyTime").IsRequired();
                c.Property(x => x.MajorID).HasColumnName("MajorID").IsRequired();

                // Quan hệ: Class -> Major
                c.HasOne(x => x.Major)
                 .WithMany(m => m.Classes)
                 .HasForeignKey(x => x.MajorID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== MAJOR ==========
            modelBuilder.Entity<Major>(m =>
            {
                m.ToTable("Major");
                m.HasKey(x => x.MajorID);
                m.Property(x => x.MajorName).HasColumnName("MajorName").HasMaxLength(100).IsRequired();
                m.Property(x => x.Description).HasColumnName("Description").HasMaxLength(500);
            });

            // ========== DEPARTMENT ==========
            modelBuilder.Entity<Department>(d =>
            {
                d.ToTable("Department");
                d.HasKey(x => x.DepartmentID);
                d.Property(x => x.DepartmentName).HasMaxLength(100).IsRequired();
                d.Property(x => x.Description).HasMaxLength(255);
            });

            // ========== LECTURER ==========
            modelBuilder.Entity<Lecturer>(l =>
            {
                l.ToTable("Lecturer");
                l.HasKey(x => x.LecturerID);
                l.Property(x => x.FullName).HasColumnName("FullName").HasMaxLength(100).IsRequired();
                l.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100).IsRequired();
                l.Property(x => x.Phone).HasColumnName("Phone").HasMaxLength(20);
                l.Property(x => x.DepartmentID).HasColumnName("DepartmentID");

                // Quan hệ: Lecturer -> Department
                l.HasOne(x => x.Department)
                 .WithMany(d => d.Lecturers)
                 .HasForeignKey(x => x.DepartmentID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== COURSE ==========
            modelBuilder.Entity<Course>(c =>
            {
                c.ToTable("Course");
                c.HasKey(x => x.CourseID);
                c.Property(x => x.CourseName).HasColumnName("CourseName").HasMaxLength(100).IsRequired();
                c.Property(x => x.Credits).HasColumnName("Credits").IsRequired();
                c.Property(x => x.Description).HasColumnName("Description").HasMaxLength(255);
                c.Property(x => x.LecturerID).HasColumnName("LecturerID");

                // Quan hệ: Course -> Lecturer
                c.HasOne(x => x.Lecturer)
                 .WithMany(l => l.Courses)
                 .HasForeignKey(x => x.LecturerID)
                 .OnDelete(DeleteBehavior.SetNull);
            });

            // ========== ENROLLMENT ==========
            modelBuilder.Entity<Enrollment>(e =>
            {
                e.ToTable("Enrollment");
                e.HasKey(x => x.EnrollmentID);
                e.Property(x => x.StudentID).HasColumnName("StudentID").IsRequired();
                e.Property(x => x.CourseID).HasColumnName("CourseID").IsRequired();
                e.Property(x => x.Semester).HasColumnName("Semester").HasMaxLength(20);
                e.Property(x => x.AcademicYear).HasColumnName("AcademicYear").HasMaxLength(20);
                e.Property(x => x.Score).HasColumnName("Score");

                // Quan hệ: Enrollment -> Student
                e.HasOne(x => x.Student)
                 .WithMany(s => s.Enrollments)
                 .HasForeignKey(x => x.StudentID)
                 .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ: Enrollment -> Course
                e.HasOne(x => x.Course)
                 .WithMany(c => c.Enrollments)
                 .HasForeignKey(x => x.CourseID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ATTENDANCE ==========
            modelBuilder.Entity<Attendance>(a =>
            {
                a.ToTable("Attendance");
                a.HasKey(x => x.AttendanceID);
                a.Property(x => x.StudentID).HasColumnName("StudentID").IsRequired();
                a.Property(x => x.CourseID).HasColumnName("CourseID").IsRequired();
                a.Property(x => x.Date).HasColumnName("Date").IsRequired();
                a.Property(x => x.Status).HasColumnName("Status").HasMaxLength(20);

                // Quan hệ: Attendance -> Student
                a.HasOne(x => x.Student)
                 .WithMany(s => s.Attendances)
                 .HasForeignKey(x => x.StudentID)
                 .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ: Attendance -> Course
                a.HasOne(x => x.Course)
                 .WithMany(c => c.Attendances)
                 .HasForeignKey(x => x.CourseID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ASSIGNMENT ==========
            modelBuilder.Entity<Assignment>(a =>
            {
                a.ToTable("Assignment");
                a.HasKey(x => x.AssignmentID);
                a.Property(x => x.CourseID).HasColumnName("CourseID").IsRequired();
                a.Property(x => x.LecturerID).HasColumnName("LecturerID").IsRequired();
                a.Property(x => x.Title).HasColumnName("Title").HasMaxLength(200).IsRequired();
                a.Property(x => x.Description).HasColumnName("Description");
                a.Property(x => x.StartDate).HasColumnName("StartDate").IsRequired();
                a.Property(x => x.EndDate).HasColumnName("EndDate").IsRequired();

                // Quan hệ: Assignment -> Course
                a.HasOne(x => x.Course)
                 .WithMany(c => c.Assignments)
                 .HasForeignKey(x => x.CourseID)
                 .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ: Assignment -> Lecturer
                a.HasOne(x => x.Lecturer)
                 .WithMany(l => l.Assignments)
                 .HasForeignKey(x => x.LecturerID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ASSIGNMENTATTACHMENT ==========
            modelBuilder.Entity<AssignmentAttachment>(a =>
            {
                a.ToTable("AssignmentAttachment");
                a.HasKey(x => x.AttachmentID);
                a.Property(x => x.AssignmentID).HasColumnName("AssignmentID").IsRequired();
                a.Property(x => x.FilePath).HasColumnName("FilePath").HasMaxLength(500).IsRequired();

                // Quan hệ: AssignmentAttachment -> Assignment
                a.HasOne(x => x.Assignment)
                 .WithMany(a => a.AssignmentAttachments)
                 .HasForeignKey(x => x.AssignmentID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ASSIGNMENTSUBMISSION ==========
            modelBuilder.Entity<AssignmentSubmission>(a =>
            {
                a.ToTable("AssignmentSubmission");
                a.HasKey(x => x.SubmissionID);
                a.Property(x => x.AssignmentID).HasColumnName("AssignmentID").IsRequired();
                a.Property(x => x.StudentID).HasColumnName("StudentID").IsRequired();
                a.Property(x => x.SubmitDate).HasColumnName("SubmitDate").IsRequired();
                a.Property(x => x.FilePath).HasColumnName("FilePath").HasMaxLength(500);
                a.Property(x => x.SubmissionText).HasColumnName("AnswerText");
                a.Property(x => x.Score).HasColumnName("Score");
                a.Property(x => x.Feedback).HasColumnName("Feedback");

                // Quan hệ: AssignmentSubmission -> Assignment
                a.HasOne(x => x.Assignment)
                 .WithMany(a => a.AssignmentSubmissions)
                 .HasForeignKey(x => x.AssignmentID)
                 .OnDelete(DeleteBehavior.Cascade);

                // Quan hệ: AssignmentSubmission -> Student
                a.HasOne(x => x.Student)
                 .WithMany(s => s.AssignmentSubmissions)
                 .HasForeignKey(x => x.StudentID)
                 .OnDelete(DeleteBehavior.Cascade);
            });

            // ========== ACCOUNT ==========
            modelBuilder.Entity<Account>(A =>
            {
                A.ToTable("Account");
                A.HasKey(a => a.UserID);

                A.Property(a => a.Username)
                 .HasColumnName("Username")
                 .HasMaxLength(50)
                 .IsRequired();
                A.HasIndex(a => a.Username).IsUnique();

                A.Property(a => a.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(255).IsRequired();
                A.Property(a => a.Role).HasColumnName("Role").HasMaxLength(20).IsRequired();
                A.Property(a => a.StudentID).IsRequired(false);
                A.Property(a => a.LecturerID).IsRequired(false);

                // Quan hệ: Account -> Student
                A.HasOne(a => a.Student)
                 .WithMany()
                 .HasForeignKey(a => a.StudentID)
                 .OnDelete(DeleteBehavior.Restrict);

                // Quan hệ: Account -> Lecturer
                A.HasOne(a => a.Lecturer)
                 .WithMany()
                 .HasForeignKey(a => a.LecturerID)
                 .OnDelete(DeleteBehavior.Restrict);
            });
            modelBuilder.Entity<Timetable>(t =>
            {
                t.ToTable("Timetable");
                t.HasKey(x => x.TimetableID);
                t.Property(x => x.ClassID).HasColumnName("ClassID").IsRequired();
                t.Property(x => x.CourseID).HasColumnName("CourseID").IsRequired();
                t.Property(x => x.StudyDate).HasColumnName("StudyDate").IsRequired();
                t.Property(x => x.StartTime).HasColumnName("StartTime").IsRequired();
                t.Property(x => x.EndTime).HasColumnName("EndTime").IsRequired();
                t.Property(x => x.Room).HasColumnName("Room").HasMaxLength(100);
                // Quan hệ: Timetable -> Class
                t.HasOne(x => x.Class)
                 .WithMany(c => c.Timetable)
                 .HasForeignKey(x => x.ClassID)
                 .OnDelete(DeleteBehavior.Cascade);
                // Quan hệ: Timetable -> Course
                t.HasOne(x => x.Course)
                 .WithMany(c => c.Timetable)
                 .HasForeignKey(x => x.CourseID)
                 .OnDelete(DeleteBehavior.Cascade);
            });
            // ========== NOTIFICATION ==========
            modelBuilder.Entity<Notification>(e =>
            {
                e.ToTable("Notification");
                e.HasKey(x => x.NotificationID);

                e.Property(x => x.RecipientID)
                    .HasColumnName("RecipientID")
                    .IsRequired();

                e.Property(x => x.Title)
                    .HasColumnName("Title")
                    .HasMaxLength(100)
                    .IsRequired();

                e.Property(x => x.Message)
                    .HasColumnName("Message")
                    .IsRequired();

                e.Property(x => x.Type)
                    .HasColumnName("Type")
                    .HasConversion<string>()
                    .IsRequired();

                e.Property(x => x.Status)
                    .HasColumnName("Status")
                    .HasConversion<string>()
                    .IsRequired();

                e.Property(x => x.CreatedDate)
                    .HasColumnName("CreatedDate")
                    .HasDefaultValueSql("GETUTCDATE()");

                e.Property(x => x.SentDate)
                    .HasColumnName("SentDate")
                    .IsRequired(false);

                e.Property(x => x.RecipientEmail)
                    .HasColumnName("RecipientEmail")
                    .HasMaxLength(255);

                e.Property(x => x.RecipientPhone)
                    .HasColumnName("RecipientPhone")
                    .HasMaxLength(20);

                e.Property(x => x.ErrorMessage)
                    .HasColumnName("ErrorMessage");

                e.Property(x => x.Priority)
                    .HasColumnName("Priority")
                    .HasMaxLength(50);
            });

            // ========== NOTIFICATION LOG ==========
            modelBuilder.Entity<NotificationLog>(e =>
            {
                e.ToTable("NotificationLog");
                e.HasKey(x => x.LogID);

                e.Property(x => x.NotificationID)
                    .HasColumnName("NotificationID")
                    .IsRequired();

                e.Property(x => x.LogDate)
                    .HasColumnName("LogDate")
                    .HasDefaultValueSql("GETUTCDATE()");

                e.Property(x => x.Action)
                    .HasColumnName("Action")
                    .HasMaxLength(50);

                e.Property(x => x.Details)
                    .HasColumnName("Details");

                e.HasOne(x => x.Notification)
                    .WithMany()
                    .HasForeignKey(x => x.NotificationID)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        } 
    }
}