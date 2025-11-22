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

        public DbSet<Student> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<Course> Courses { get; set; }
        public DbSet<Assignment> Assignments { get; set; }
        public DbSet<Account> Accounts { get; set; }

        // override OnModelCreating if you need custom mapping
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            // example: modelBuilder.Entity<Student>().ToTable("Student");
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
            // 1. Thiết lập mối quan hệ: Student - StudentClass
            modelBuilder.Entity<StudentClass>(entity =>
                {
                    entity.ToTable("StudentClass");
                    entity.HasKey(sc => sc.StudentClassID); // Khóa chính của bảng trung gian
                    // 1. Quan hệ: StudentClass -> Student
                    entity.HasOne(sc => sc.Student)
                          .WithMany(s => s.StudentClasses)     // Student có nhiều StudentClasses
                          .HasForeignKey(sc => sc.StudentId)
                          .OnDelete(DeleteBehavior.Cascade);
                    // 2. Quan hệ: StudentClass -> Class
                    entity.HasOne(sc => sc.Class)
                          .WithMany(c => c.StudentClasses)     // Class có nhiều StudentClasses
                          .HasForeignKey(sc => sc.ClassID)
                          .OnDelete(DeleteBehavior.Cascade);
                });
            modelBuilder.Entity<Class>(c =>
            {
                c.ToTable("Class"); // Tên bảng trong SQL
                // 1. Khóa chính
                c.HasKey(x => x.ClassID);
                // 2. Các thuộc tính cơ bản
                c.Property(x => x.ClassName).HasColumnName("ClassName").HasMaxLength(100).IsRequired();
                c.Property(x => x.StudyTime).HasColumnName("StudyTime").HasMaxLength(20).IsRequired(); 
                c.Property(x => x.MajorID).HasColumnName("MajorID").IsRequired();
                // 4. Thiết lập mối quan hệ với bảng Major
                // "Một Lớp (Class) thuộc về Một Ngành (Major)"
                c.HasOne(x => x.Major)
                 .WithMany(m => m.Classes)         
                 .HasForeignKey(x => x.MajorID)    // Khóa ngoại liên kết
                 .OnDelete(DeleteBehavior.Cascade);// Xóa Ngành -> Xóa luôn các Lớp thuộc ngành đó (tùy chọn)
            });
            modelBuilder.Entity<Major>(m =>
            {
                m.ToTable("Major"); // Tên bảng trong SQL
                // 1. Khóa chính
                m.HasKey(x => x.MajorID);
                // 2. Các thuộc tính cơ bản
                m.Property(x => x.MajorName).HasColumnName("MajorName").HasMaxLength(100).IsRequired();
                m.Property(x => x.Description).HasColumnName("Description").HasMaxLength(500).IsRequired();
            });// Add other entities similarly
               // Thêm cấu hình này vào OnModelCreating
            modelBuilder.Entity<Department>(d =>
            {
                d.ToTable("Department");
                d.HasKey(x => x.DepartmentID);
                d.Property(x => x.DepartmentName)
                 .HasMaxLength(100)
                 .IsRequired();
                // Không cần cấu hình lại HasMany ở đây vì đã cấu hình bên Lecturer rồi
                // EF Core sẽ tự hiểu 2 chiều.
            });
            modelBuilder.Entity<Lecturer>(l =>
            {
                l.ToTable("Lecturer");
                // Khóa chính
                l.HasKey(x => x.LecturerID);
                l.Property(x => x.FullName).HasColumnName("FullName").HasMaxLength(100).IsRequired();
                l.Property(x => x.Email).HasColumnName("Email").HasMaxLength(100).IsRequired();
                l.Property(x => x.Phone).HasColumnName("Phone").HasMaxLength(20).IsRequired();
                l.Property(x => x.DepartmentID).HasColumnName("DepartmentID").IsRequired();

                // Thiết lập quan hệ
                l.HasOne(x => x.Department)
                 .WithMany(d => d.Lecturers) 
                 .HasForeignKey(x => x.DepartmentID)
                 .OnDelete(DeleteBehavior.Cascade);
            });
            modelBuilder.Entity<Account>(A =>
            {
                A.ToTable("Account");
                // 1. Khóa chính
                A.HasKey(a => a.UserID);
                // 2. Username
                A.Property(a => a.Username)
                 .HasColumnName("Username")
                 .HasMaxLength(50)
                 .IsRequired();
                // THÊM: Ràng buộc Unique cho Username (Khớp với SQL UNIQUE)
                A.HasIndex(a => a.Username).IsUnique();

                // 3. PasswordHash
                A.Property(a => a.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(255).IsRequired();
                A.Property(a => a.Role).HasColumnName("Role").HasMaxLength(20).IsRequired();
                A.Property(a => a.StudentID).IsRequired(false);
                A.Property(a => a.LecturerID).IsRequired(false);
                // 6. Thiết lập mối quan hệ (Nếu trong Model Account có biến Student và Lecturer)
                A.HasOne(a => a.Student)
                 .WithMany() // Hoặc .WithOne() nếu thiết lập 1-1
                 .HasForeignKey(a => a.StudentID)
                 .OnDelete(DeleteBehavior.Restrict); // Tránh xóa nhầm
                A.HasOne(a => a.Lecturer)
                 .WithMany()
                 .HasForeignKey(a => a.LecturerID)
                 .OnDelete(DeleteBehavior.Restrict);
            });
        }


    }
}
