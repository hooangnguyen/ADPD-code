-- =====================================
-- 1. TABLE: Department (Khoa)
-- =====================================
CREATE TABLE Department (
    DepartmentID INT IDENTITY PRIMARY KEY,
    DepartmentName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);

-- =====================================
-- 2. TABLE: Major (Ngành học)
-- =====================================
CREATE TABLE Major (
    MajorID INT IDENTITY PRIMARY KEY,
    MajorName NVARCHAR(100) NOT NULL,
    Description NVARCHAR(255)
);

-- =====================================
-- 3. TABLE: Class (Lớp học)
-- =====================================
CREATE TABLE Class (
    ClassID INT IDENTITY PRIMARY KEY,
    ClassName NVARCHAR(100) NOT NULL,
    StudyTime Datetime,
    MajorID INT,
    FOREIGN KEY (MajorID) REFERENCES Major(MajorID)
);

-- =====================================
-- 4. TABLE: Lecturer (Giảng viên)
-- =====================================
CREATE TABLE Lecturer (
    LecturerID INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100),
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    DepartmentID INT,
    FOREIGN KEY (DepartmentID) REFERENCES Department(DepartmentID)
);

-- =====================================
-- 5. TABLE: Student (Sinh viên)
-- *** ĐÃ BỎ ClassID để thiết lập quan hệ N:M ***
-- =====================================
CREATE TABLE Student (
    StudentID INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100),
    Gender NVARCHAR(10),
    DOB DATETIME,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    Status NVARCHAR(50)
    -- ClassID đã được loại bỏ
);

-- =====================================
-- 6. TABLE: Course (Môn học)
-- =====================================
CREATE TABLE Course (
    CourseID INT IDENTITY PRIMARY KEY,
    CourseName NVARCHAR(100),
    Credits INT,
    Description NVARCHAR(255),
    LecturerID INT,
    FOREIGN KEY (LecturerID) REFERENCES Lecturer(LecturerID)
);

-- =====================================
-- 7. TABLE: Enrollment (Đăng ký tín chỉ / điểm)
-- =====================================
CREATE TABLE Enrollment (
    EnrollmentID INT IDENTITY PRIMARY KEY,
    StudentID INT,
    CourseID INT,
    Semester NVARCHAR(20),
    AcademicYear NVARCHAR(20),
    Score FLOAT,
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (CourseID) REFERENCES Course(CourseID)
);

-- =====================================
-- 8. TABLE: Attendance (Điểm danh)
-- =====================================
CREATE TABLE Attendance (
    AttendanceID INT IDENTITY PRIMARY KEY,
    StudentID INT,
    CourseID INT,
    Date DATE,
    Status NVARCHAR(20),
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (CourseID) REFERENCES Course(CourseID)
);

-- =====================================
-- 9. TABLE: Account (Đăng nhập + phân quyền)
-- =====================================
CREATE TABLE Account (
    UserID INT IDENTITY PRIMARY KEY,
    Username NVARCHAR(50) UNIQUE NOT NULL,
    PasswordHash NVARCHAR(255) NOT NULL,
    Role NVARCHAR(20) NOT NULL,  -- Admin / Student / Lecturer
    StudentID INT NULL,
    LecturerID INT NULL,
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (LecturerID) REFERENCES Lecturer(LecturerID)
);

-- =====================================
-- 10. TABLE: Assignment (Giảng viên giao bài tập)
-- =====================================
CREATE TABLE Assignment (
    AssignmentID INT IDENTITY PRIMARY KEY,
    CourseID INT NOT NULL,
    LecturerID INT NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX),
    StartDate DATETIME NOT NULL,
    EndDate DATETIME NOT NULL,
    FOREIGN KEY (CourseID) REFERENCES Course(CourseID),
    FOREIGN KEY (LecturerID) REFERENCES Lecturer(LecturerID)
);

-- =====================================
-- 11. TABLE: AssignmentAttachment (File đính kèm bài tập)
-- =====================================
CREATE TABLE AssignmentAttachment (
    AttachmentID INT IDENTITY PRIMARY KEY,
    AssignmentID INT NOT NULL,
    FilePath NVARCHAR(500) NOT NULL,
    FOREIGN KEY (AssignmentID) REFERENCES Assignment(AssignmentID)
);

-- =====================================
-- 12. TABLE: AssignmentSubmission (Sinh viên nộp bài)
-- =====================================
CREATE TABLE AssignmentSubmission (
    SubmissionID INT IDENTITY PRIMARY KEY,
    AssignmentID INT NOT NULL,
    StudentID INT NOT NULL,
    SubmitDate DATETIME NOT NULL,
    FilePath NVARCHAR(500),
    AnswerText NVARCHAR(MAX),
    Score FLOAT NULL,
    Feedback NVARCHAR(MAX),
    FOREIGN KEY (AssignmentID) REFERENCES Assignment(AssignmentID),
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID)
);

-- =====================================
-- 13. TABLE: StudentClass (Sinh viên - Lớp học)
-- *** BẢNG TRUNG GIAN MỚI cho quan hệ N:M ***
-- =====================================
CREATE TABLE StudentClass (
    StudentClassID INT IDENTITY PRIMARY KEY,
    StudentID INT NOT NULL,
    ClassID INT NOT NULL,
    FOREIGN KEY (StudentID) REFERENCES Student(StudentID),
    FOREIGN KEY (ClassID) REFERENCES Class(ClassID),
    UNIQUE (StudentID, ClassID) -- Đảm bảo cặp (SV, Lớp) là duy nhất
);
CREATE TABLE Timetable (
    TimetableID INT IDENTITY PRIMARY KEY,
    ClassID INT NOT NULL,
    CourseID INT NOT NULL,
    StudyDate DATE NOT NULL,
    StartTime TIME NOT NULL,
    EndTime TIME NOT NULL,
    Room NVARCHAR(100),
    FOREIGN KEY (ClassID) REFERENCES Class(ClassID),
    FOREIGN KEY (CourseID) REFERENCES Course(CourseID)
);

-- Dữ liệu cho Department
SET IDENTITY_INSERT Department ON; -- Cần thiết nếu bạn muốn tự đặt ID (không bắt buộc với IDENTITY)
INSERT INTO Department (DepartmentID, DepartmentName) VALUES
(1, N'Khoa Công nghệ Thông tin'),
(2, N'Khoa Kinh tế'),
(3, N'Khoa Kỹ thuật Điện');
SET IDENTITY_INSERT Department OFF;

-- Dữ liệu cho Major
SET IDENTITY_INSERT Major ON;
INSERT INTO Major (MajorID, MajorName, Description) VALUES
(1, 'Computer Science', N'Nghiên cứu về khoa học máy tính và lập trình.'),
(2, 'Business Administration', N'Nghiên cứu về quản lý và vận hành doanh nghiệp.'),
(3, 'Electrical Engineering', N'Nghiên cứu về kỹ thuật điện và điện tử.');
SET IDENTITY_INSERT Major OFF;

-- Dữ liệu cho Lecturer
SET IDENTITY_INSERT Lecturer ON;
INSERT INTO Lecturer (LecturerID, FullName, Email, Phone, DepartmentID) VALUES
(1, N'Nguyễn Văn A', 'anv@university.edu.vn', '0901111111', 1),
(2, N'Trần Thị B', 'btt@university.edu.vn', '0902222222', 1),
(3, N'Lê Văn C', 'clv@university.edu.vn', '0903333333', 2),
(4, N'Phạm Thị D', 'dpth@university.edu.vn', '0904444444', 3);
SET IDENTITY_INSERT Lecturer OFF;

-- Dữ liệu cho Class
SET IDENTITY_INSERT Class ON;
INSERT INTO Class (ClassID, ClassName, StudyTime, MajorID) VALUES
(1, N'CS2025A', '2025-09-05 08:00:00', 1),
(2, N'BA2025B', '2025-09-05 08:00:00', 2),
(3, N'EE2025C', '2025-09-05 08:00:00', 3);
SET IDENTITY_INSERT Class OFF;

-- Dữ liệu cho Student
SET IDENTITY_INSERT Student ON;
INSERT INTO Student (StudentID, FullName, Gender, DOB, Email, Phone, Address, Status) VALUES
(1, N'Hồ Hoàng Q', N'Nam', '2005-01-15', 'hoangq@gmail.com', '0911223344', N'Thủ Đức, TP.HCM', N'Active'),
(2, N'Phan Thị K', N'Nữ', '2005-03-20', 'phanthik@gmail.com', '0911223355', N'Quận 1, TP.HCM', N'Active'),
(3, N'Đặng Văn L', N'Nam', '2004-11-01', 'vanl@gmail.com', '0911223366', N'Quận 3, TP.HCM', N'Active'),
(4, N'Võ Thị M', N'Nữ', '2006-05-25', 'vom@gmail.com', '0911223377', N'Quận 5, TP.HCM', N'Active');
SET IDENTITY_INSERT Student OFF;

-- Dữ liệu cho Course
SET IDENTITY_INSERT Course ON;
INSERT INTO Course (CourseID, CourseName, Credits, Description, LecturerID) VALUES
(1, N'Cơ sở Dữ liệu', 3, N'Lý thuyết và thực hành SQL.', 1),
(2, N'Lập trình Hướng đối tượng', 4, N'Giới thiệu về OOP.', 2),
(3, N'Kinh tế Vi mô', 3, N'Nguyên lý kinh tế cơ bản.', 3),
(4, N'Mạch Điện tử', 4, N'Phân tích và thiết kế mạch điện.', 4);
SET IDENTITY_INSERT Course OFF;

-- Dữ liệu cho StudentClass
INSERT INTO StudentClass (StudentID, ClassID) VALUES
(1, 1),
(2, 1),
(3, 2),
(4, 3),
(1, 2);

-- Dữ liệu cho Timetable
INSERT INTO Timetable (ClassID, CourseID, StudyDate, StartTime, EndTime, Room) VALUES
(1, 1, '2025-11-10', '08:00:00', '10:30:00', 'Room A101'),
(1, 2, '2025-11-12', '13:00:00', '16:00:00', 'Room A102'),
(2, 3, '2025-11-10', '10:30:00', '13:00:00', 'Room B201'),
(3, 4, '2025-11-13', '08:00:00', '11:00:00', 'Room C301');

-- Dữ liệu cho Enrollment
INSERT INTO Enrollment (StudentID, CourseID, Semester, AcademicYear, Score) VALUES
(1, 1, N'Kỳ 1', '2025-2026', 8.5),
(1, 2, N'Kỳ 1', '2025-2026', 7.0),
(2, 1, N'Kỳ 1', '2025-2026', 9.2),
(3, 3, N'Kỳ 1', '2025-2026', 7.5),
(4, 4, N'Kỳ 1', '2025-2026', NULL);

-- Dữ liệu cho Attendance
INSERT INTO Attendance (StudentID, CourseID, Date, Status) VALUES
(1, 1, '2025-11-10', N'Present'),
(2, 1, '2025-11-10', N'Present'),
(3, 3, '2025-11-10', N'Present'),
(1, 2, '2025-11-12', N'Absent'),
(4, 4, '2025-11-13', N'Present');

-- Dữ liệu cho Account
INSERT INTO Account (Username, PasswordHash, Role, StudentID, LecturerID) VALUES
('admin', 'hashed_admin_password', 'Admin', NULL, NULL),
('hoangq', 'hashed_student1_password', 'Student', 1, NULL),
('phanthik', 'hashed_student2_password', 'Student', 2, NULL),
('nguyenvana', 'hashed_lecturer1_password', 'Lecturer', NULL, 1);

-- Dữ liệu cho Assignment
SET IDENTITY_INSERT Assignment ON;
INSERT INTO Assignment (AssignmentID, CourseID, LecturerID, Title, Description, StartDate, EndDate) VALUES
(1, 1, 1, N'Bài tập lớn CSDL', N'Thiết kế và triển khai CSDL cho hệ thống quản lý thư viện.', '2025-11-15 09:00:00', '2025-12-15 23:59:59'),
(2, 2, 2, N'Project OOP', N'Xây dựng ứng dụng quản lý sinh viên bằng Java/C#.', '2025-11-20 08:00:00', '2025-12-20 23:59:59');
SET IDENTITY_INSERT Assignment OFF;

-- Dữ liệu cho AssignmentAttachment
INSERT INTO AssignmentAttachment (AssignmentID, FilePath) VALUES
(1, '/assignments/csdl/yeucau_bttl.pdf'),
(2, '/assignments/oop/template_project.zip');

-- Dữ liệu cho AssignmentSubmission
INSERT INTO AssignmentSubmission (AssignmentID, StudentID, SubmitDate, FilePath, AnswerText, Score, Feedback) VALUES
(1, 1, '2025-12-10 15:30:00', '/submissions/bttl/hoangq_csdl.zip', NULL, 9.0, N'Bài làm tốt, triển khai đầy đủ các yêu cầu.'),
(1, 2, '2025-12-14 20:00:00', NULL, N'Đã nộp bài tập lớn qua hệ thống khác.', 8.5, N'Đầy đủ, cần chú ý chuẩn hóa thêm.'),
(2, 1, '2025-12-18 10:00:00', '/submissions/oop/hoangq_oop_project.zip', NULL, NULL, NULL);

