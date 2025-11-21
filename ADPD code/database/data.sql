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
    DOB DATE,
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
-- =====================================
-- INSERT DỮ LIỆU MẪU
-- =====================================

-- 1. Department (Khoa)
INSERT INTO Department (DepartmentName, Description) VALUES
(N'Khoa Công nghệ Thông tin', N'Đào tạo về phần mềm, mạng máy tính, AI'),
(N'Khoa Kinh tế', N'Đào tạo về kinh doanh, tài chính, kế toán'),
(N'Khoa Ngoại ngữ', N'Đào tạo tiếng Anh, tiếng Nhật, tiếng Trung'),
(N'Khoa Cơ khí', N'Đào tạo kỹ thuật cơ khí, chế tạo máy'),
(N'Khoa Điện - Điện tử', N'Đào tạo điện tử viễn thông, tự động hóa');

-- 2. Major (Ngành học)
INSERT INTO Major (MajorName, Description) VALUES
(N'Công nghệ Phần mềm', N'Phát triển ứng dụng, hệ thống phần mềm'),
(N'Khoa học Máy tính', N'Lý thuyết máy tính, thuật toán, AI'),
(N'Quản trị Kinh doanh', N'Quản lý doanh nghiệp, marketing'),
(N'Kế toán', N'Kế toán tài chính, kiểm toán'),
(N'Tiếng Anh', N'Ngôn ngữ Anh, biên phiên dịch'),
(N'Kỹ thuật Cơ khí', N'Thiết kế, chế tạo máy móc'),
(N'Điện tử Viễn thông', N'Thiết bị điện tử, truyền thông');

-- 3. Class (Lớp học)
INSERT INTO Class (ClassName, StudyTime, MajorID) VALUES
(N'CNPM01', '2023-09-01 08:00:00', 1),
(N'CNPM02', '2023-09-01 08:00:00', 1),
(N'KHMT01', '2023-09-01 08:00:00', 2),
(N'QTKD01', '2023-09-01 08:00:00', 3),
(N'KTTC01', '2023-09-01 08:00:00', 4),
(N'TA01', '2023-09-01 08:00:00', 5),
(N'KTCK01', '2023-09-01 08:00:00', 6),
(N'DTVT01', '2023-09-01 08:00:00', 7);

-- 4. Lecturer (Giảng viên)
INSERT INTO Lecturer (FullName, Email, Phone, DepartmentID) VALUES
(N'Nguyễn Văn An', 'nguyenvanan@university.edu.vn', '0901234567', 1),
(N'Trần Thị Bình', 'tranthibinh@university.edu.vn', '0902345678', 1),
(N'Lê Minh Cường', 'leminhcuong@university.edu.vn', '0903456789', 1),
(N'Phạm Thị Dung', 'phamthidung@university.edu.vn', '0904567890', 2),
(N'Hoàng Văn Em', 'hoangvanem@university.edu.vn', '0905678901', 2),
(N'Vũ Thị Phượng', 'vuthiphuong@university.edu.vn', '0906789012', 3),
(N'Đặng Minh Giang', 'dangminhgiang@university.edu.vn', '0907890123', 4),
(N'Bùi Văn Hùng', 'buivanhung@university.edu.vn', '0908901234', 5);

-- 5. Student (Sinh viên)
INSERT INTO Student (FullName, Gender, DOB, Email, Phone, Address, Status) VALUES
(N'Nguyễn Minh Anh', N'Nam', '2004-03-15', 'minhanh@student.edu.vn', '0911111111', N'Hà Nội', N'Đang học'),
(N'Trần Thùy Dương', N'Nữ', '2004-05-20', 'thuyduong@student.edu.vn', '0912222222', N'Hồ Chí Minh', N'Đang học'),
(N'Lê Hoàng Phúc', N'Nam', '2004-07-10', 'hoangphuc@student.edu.vn', '0913333333', N'Đà Nẵng', N'Đang học'),
(N'Phạm Ngọc Lan', N'Nữ', '2004-01-25', 'ngoclan@student.edu.vn', '0914444444', N'Hải Phòng', N'Đang học'),
(N'Hoàng Tuấn Kiệt', N'Nam', '2004-09-05', 'tuankiet@student.edu.vn', '0915555555', N'Cần Thơ', N'Đang học'),
(N'Vũ Thu Hà', N'Nữ', '2004-11-18', 'thuha@student.edu.vn', '0916666666', N'Huế', N'Đang học'),
(N'Đỗ Minh Quân', N'Nam', '2004-02-28', 'minhquan@student.edu.vn', '0917777777', N'Nha Trang', N'Đang học'),
(N'Bùi Thanh Mai', N'Nữ', '2004-04-12', 'thanhmai@student.edu.vn', '0918888888', N'Hà Nội', N'Đang học'),
(N'Đinh Văn Tùng', N'Nam', '2004-06-08', 'vantung@student.edu.vn', '0919999999', N'Hồ Chí Minh', N'Đang học'),
(N'Lý Thu Thảo', N'Nữ', '2004-08-22', 'thuthao@student.edu.vn', '0910000000', N'Đà Nẵng', N'Đang học'),
(N'Phan Đức Long', N'Nam', '2003-12-30', 'duclong@student.edu.vn', '0910111111', N'Hà Nội', N'Bảo lưu'),
(N'Ngô Hương Giang', N'Nữ', '2004-10-14', 'huonggiang@student.edu.vn', '0910222222', N'Hải Phòng', N'Đang học');

-- 6. Course (Môn học)
INSERT INTO Course (CourseName, Credits, Description, LecturerID) VALUES
(N'Lập trình C/C++', 4, N'Ngôn ngữ lập trình cơ bản', 1),
(N'Cấu trúc dữ liệu và Giải thuật', 4, N'Cấu trúc dữ liệu, thuật toán', 1),
(N'Cơ sở dữ liệu', 3, N'Thiết kế và quản trị CSDL', 2),
(N'Lập trình Web', 3, N'HTML, CSS, JavaScript, Backend', 3),
(N'Trí tuệ nhân tạo', 4, N'Machine Learning, Deep Learning', 3),
(N'Kinh tế vi mô', 3, N'Các khái niệm kinh tế cơ bản', 4),
(N'Quản trị Marketing', 3, N'Chiến lược marketing hiện đại', 5),
(N'Tiếng Anh chuyên ngành', 2, N'Tiếng Anh cho IT/Business', 6),
(N'Kỹ thuật Cơ khí', 4, N'Nguyên lý cơ khí, vẽ kỹ thuật', 7),
(N'Mạch điện tử', 3, N'Thiết kế mạch số và tương tự', 8);

-- 7. Enrollment (Đăng ký môn học và điểm)
INSERT INTO Enrollment (StudentID, CourseID, Semester, AcademicYear, Score) VALUES
-- Sinh viên 1
(1, 1, N'HK1', N'2023-2024', 8.5),
(1, 2, N'HK1', N'2023-2024', 7.8),
(1, 3, N'HK1', N'2023-2024', 9.0),
-- Sinh viên 2
(2, 1, N'HK1', N'2023-2024', 7.5),
(2, 3, N'HK1', N'2023-2024', 8.0),
(2, 4, N'HK1', N'2023-2024', 8.5),
-- Sinh viên 3
(3, 1, N'HK1', N'2023-2024', 9.0),
(3, 2, N'HK1', N'2023-2024', 8.8),
(3, 5, N'HK1', N'2023-2024', 7.5),
-- Sinh viên 4
(4, 6, N'HK1', N'2023-2024', 8.2),
(4, 7, N'HK1', N'2023-2024', 7.9),
(4, 8, N'HK1', N'2023-2024', 8.5),
-- Sinh viên 5
(5, 1, N'HK1', N'2023-2024', 6.5),
(5, 3, N'HK1', N'2023-2024', 7.0),
-- Sinh viên 6-10
(6, 8, N'HK1', N'2023-2024', 9.0),
(7, 9, N'HK1', N'2023-2024', 7.8),
(8, 1, N'HK1', N'2023-2024', 8.3),
(9, 10, N'HK1', N'2023-2024', 7.5),
(10, 1, N'HK1', N'2023-2024', 8.8);

-- 8. Attendance (Điểm danh)
INSERT INTO Attendance (StudentID, CourseID, Date, Status) VALUES
-- Tuần 1
(1, 1, '2023-09-04', N'Có mặt'),
(1, 2, '2023-09-05', N'Có mặt'),
(2, 1, '2023-09-04', N'Có mặt'),
(3, 1, '2023-09-04', N'Vắng có phép'),
-- Tuần 2
(1, 1, '2023-09-11', N'Có mặt'),
(2, 1, '2023-09-11', N'Vắng không phép'),
(3, 1, '2023-09-11', N'Có mặt'),
-- Tuần 3
(1, 1, '2023-09-18', N'Có mặt'),
(2, 1, '2023-09-18', N'Có mặt'),
(3, 1, '2023-09-18', N'Có mặt');

-- 9. Account (Tài khoản đăng nhập)
INSERT INTO Account (Username, PasswordHash, Role, StudentID, LecturerID) VALUES
-- Admin
('admin', '123', 'Admin', NULL, NULL),
-- Giảng viên
('gv_nguyenvanan', '123', 'Lecturer', NULL, 1),
('gv_tranthibinh', '123', 'Lecturer', NULL, 2),
('gv_leminhcuong', '123', 'Lecturer', NULL, 3),
-- Sinh viên
('sv_minhanh', '123', 'Student', 1, NULL),
('sv_thuyduong', '123', 'Student', 2, NULL),
('sv_hoangphuc', '123', 'Student', 3, NULL),
('sv_ngoclan', '123', 'Student', 4, NULL),
('sv_tuankiet', '123', 'Student', 5, NULL);

-- 10. Assignment (Bài tập)
INSERT INTO Assignment (CourseID, LecturerID, Title, Description, StartDate, EndDate) VALUES
(1, 1, N'Bài tập 1: Nhập môn C++', N'Viết chương trình Hello World và các phép toán cơ bản', '2023-09-05 00:00:00', '2023-09-15 23:59:59'),
(1, 1, N'Bài tập 2: Vòng lặp và mảng', N'Thực hành về vòng lặp for, while và mảng', '2023-09-16 00:00:00', '2023-09-25 23:59:59'),
(2, 1, N'Bài tập 1: Stack và Queue', N'Cài đặt cấu trúc dữ liệu Stack và Queue', '2023-09-10 00:00:00', '2023-09-20 23:59:59'),
(3, 2, N'Thiết kế CSDL', N'Thiết kế cơ sở dữ liệu cho hệ thống quản lý thư viện', '2023-09-07 00:00:00', '2023-09-21 23:59:59'),
(4, 3, N'Xây dựng Website cá nhân', N'Tạo website giới thiệu bản thân bằng HTML/CSS', '2023-09-08 00:00:00', '2023-09-22 23:59:59');

-- 11. AssignmentAttachment (File đính kèm bài tập)
INSERT INTO AssignmentAttachment (AssignmentID, FilePath) VALUES
(1, '/files/assignments/bt1_huongdan.pdf'),
(1, '/files/assignments/bt1_template.cpp'),
(2, '/files/assignments/bt2_examples.pdf'),
(3, '/files/assignments/bt3_stack_queue.pdf'),
(4, '/files/assignments/bt4_erd_guide.pdf');

-- 12. AssignmentSubmission (Sinh viên nộp bài)
INSERT INTO AssignmentSubmission (AssignmentID, StudentID, SubmitDate, FilePath, AnswerText, Score, Feedback) VALUES
(1, 1, '2023-09-14 10:30:00', '/files/submissions/sv1_bt1.cpp', NULL, 9.0, N'Làm tốt, code sạch'),
(1, 2, '2023-09-15 23:45:00', '/files/submissions/sv2_bt1.cpp', NULL, 7.5, N'Đúng logic nhưng thiếu comment'),
(1, 3, '2023-09-13 15:20:00', '/files/submissions/sv3_bt1.cpp', NULL, 8.5, N'Rất tốt'),
(2, 1, '2023-09-24 18:00:00', '/files/submissions/sv1_bt2.cpp', NULL, NULL, NULL),
(3, 1, '2023-09-19 09:15:00', '/files/submissions/sv1_bt3.cpp', NULL, 8.0, N'Cài đặt đúng nhưng chưa tối ưu'),
(4, 2, '2023-09-20 14:30:00', '/files/submissions/sv2_bt4.pdf', N'Đã thiết kế ERD cho hệ thống thư viện', 8.5, N'Thiết kế hợp lý');

-- 13. StudentClass (Sinh viên - Lớp học: quan hệ N:M)
INSERT INTO StudentClass (StudentID, ClassID) VALUES
-- Lớp CNPM01
(1, 1),
(2, 1),
(3, 1),
(8, 1),
-- Lớp CNPM02
(5, 2),
(10, 2),
-- Lớp KHMT01
(3, 3),
-- Lớp QTKD01
(4, 4),
-- Lớp KTTC01
(4, 5),
-- Lớp TA01
(6, 6),
-- Lớp KTCK01
(7, 7),
-- Lớp DTVT01
(9, 8),
(12, 8);

-- =====================================
-- KẾT THÚC INSERT DỮ LIỆU MẪU
-- =====================================