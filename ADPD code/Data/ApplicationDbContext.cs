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
        }


    }
}
