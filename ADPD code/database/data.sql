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

<<<<<<< HEAD
=======
-- ================================
-- 1. XÓA DỮ LIỆU TRÙNG (AN TOÀN)
-- ================================
DELETE FROM Account 
WHERE Username IN ('admin', 'gv_hangoclinh', 'gv_nguyenthanhtrieu', 'gv_nguyenthihonghanh');

DELETE FROM Lecturer 
WHERE LecturerId IN (4, 5, 6);
-- Nếu Id tự tạo, có thể dùng:
-- DELETE FROM Lecturer WHERE FullName IN (...)


-- ================================
-- 2. THÊM 3 GIẢNG VIÊN
-- ================================
INSERT INTO Lecturer (FullName, Email, Phone, DepartmentId)
VALUES 
    (N'Hà Ngọc Linh',      'linh@example.com',  '0912345678', 1),
    (N'Nguyễn Thanh Triều','trieu@example.com', '0912345679', 1),
    (N'Nguyễn Thị Hồng Hạnh','hanh@example.com','0912345680', 2);


-- ================================
-- 3. LẤY ID CÁC GIẢNG VIÊN
-- ================================
SELECT * FROM Lecturer;


-- ================================
-- 4. THÊM TÀI KHOẢN CHO GIẢNG VIÊN (MẬT KHẨU CHƯA MÃ HÓA)
-- ================================
INSERT INTO Account (Username, PasswordHash, Role, LecturerId)
VALUES
    ('gv_hangoclinh',       '123456', 'Lecturer', (SELECT LecturerId FROM Lecturer WHERE Email='linh@example.com')),
    ('gv_nguyenthanhtrieu', '123456', 'Lecturer', (SELECT LecturerId FROM Lecturer WHERE Email='trieu@example.com')),
    ('gv_nguyenthihonghanh','123456', 'Lecturer', (SELECT LecturerId FROM Lecturer WHERE Email='hanh@example.com'));


-- ================================
-- 5. THÊM TÀI KHOẢN ADMIN
-- ================================
INSERT INTO Account (Username, PasswordHash, Role, LecturerId)
VALUES ('admin', '123456', 'Admin', NULL);


-- ================================
-- 6. KIỂM TRA KẾT QUẢ
-- ================================
SELECT * FROM Account;


>>>>>>> 54a9c6827043fd99a2ece98a3fa1275f2b5cb6d9
-- =====================================
-- KẾT THÚC INSERT DỮ LIỆU MẪU
-- =====================================
--======================================
--1. Cài đặt BCrypt
--bashcd "ADPD code"
--dotnet add package BCrypt.Net-Next
--=======================================