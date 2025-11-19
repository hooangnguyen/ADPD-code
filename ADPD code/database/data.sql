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
    StudyTime Datetime(20),
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
-- =====================================
CREATE TABLE Student (
    StudentID INT IDENTITY PRIMARY KEY,
    FullName NVARCHAR(100),
    Gender NVARCHAR(10),
    DOB DATE,
    Email NVARCHAR(100),
    Phone NVARCHAR(20),
    Address NVARCHAR(255),
    Status NVARCHAR(50),
    ClassID INT,
    FOREIGN KEY (ClassID) REFERENCES Class(ClassID)
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
    Role NVARCHAR(20) NOT NULL,    -- Admin / Student / Lecturer
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
    DueDate DATETIME NOT NULL,
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
