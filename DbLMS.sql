Chi tiết README tại: https://docs.google.com/document/d/12MqsnLSY7YkCzzbzFKreHYH1p6qxAGpDagFcmQuPZnY/edit?usp=sharing
Database LMS
Drop Database DbLMS;
Create database DbLMS;
go

Use DbLMS;
go

Create table Roles --General role set
(
	RoleID nvarchar(155) Primary Key, --Using UUIDv7
	RoleName nvarchar(155),
	Description nvarchar(155)
);
go

Create table Users --Login Infor
(
	UserID nvarchar(155) Primary Key, --Using UUIDv7
	Username nvarchar(55),
	PasswordHash nvarchar(555),
	RoleID nvarchar(155) Foreign Key (RoleID) References Roles(RoleID),
	Status nvarchar(155),
	CreateAt Datetime Default GetDate()
);
go

--Infor Institute/Departments (CQT, OUM, ...)
Create Table Departments 
(
	DepartID nvarchar(155) Primary Key,
	DepartName nvarchar(155),
	DepartDescription nvarchar(155), 
	Address nvarchar(300),
	Phone nvarchar(20),
	Image nvarchar(555),
	Status nvarchar(55),
	CreateAt DateTime Default GetDate(),
	ApprovalDate DateTime Default GetDate()
);
go

Create Table SchoolYears --Set SchoolYear harden
(
	YearID nvarchar(155) Primary Key,
	DepartID nvarchar(155) Foreign Key References Departments (DepartID),
	YearName nvarchar(50),
	StartDate date,
	EndDate date,
);
go

Create Table Semesters -- Set Semesters harden
(
	SemesterID nvarchar(155) Primary Key,
	YearID nvarchar(155) Foreign Key References SchoolYears (YearID),
	DepartID nvarchar(155) Foreign Key References Departments (DepartID),
	SemesterName nvarchar(100),
	StartDate date,
	EndDate date
);
Go

--Major for student
Create Table Majors
(
	MajorID nvarchar (155) Primary Key,
	MajorName nvarchar(155),
	MajorDescription nvarchar (155)
);
go

Create Table Specializations
(
	SpecialID nvarchar(155) Primary Key,
	MajorID nvarchar(155) Foreign Key References Majors (MajorID),
	SpecialName nvarchar (155)
);
go

Create Table DepartmentStaffs
(
	StaffID nvarchar(155) Primary Key,
	UserID nvarchar(155) Foreign key (UserID) References Users (UserID),
	SchoolID nvarchar(155) Foreign Key (SchoolID) References Departments (DepartID),
	Majors nvarchar (155) Foreign Key References Majors (MajorID),
	FirstName nvarchar(20),
	LastName nvarchar(50),
	StaffRole nvarchar(155) Foreign Key References Roles (RoleID),
	DOB Date,
	PhoneNum nvarchar(20),
	Mail nvarchar(155),
	Address nvarchar (300),
	Image nvarchar(555),
	Status nvarchar(100),
	HireDate DateTime
);
go

Create Table StaffDeparts
(
	StaffID nvarchar(155) not null,
	DepartID nvarchar(155) not null,
	Primary Key (StaffID, DepartID),
	FOREIGN KEY (StaffID) REFERENCES DepartmentStaffs(StaffID),
    FOREIGN KEY (DepartID) REFERENCES Departments(DepartID)
);
go

Create Table Classes
(
	ClassID nvarchar (155) Primary Key,
	ClassName nvarchar (50),
	ClassMajor nvarchar (155) Foreign Key References Majors (MajorID),
	DepartID nvarchar(155) Foreign Key References Departments (DepartID)
	Course nvarchar(15)
);
go

Create Table Students
(
	StudentID nvarchar(155) Primary Key,
	UserID nvarchar(155) Foreign Key (UserID) References Users (UserID),
	DepartID nvarchar(155) Foreign Key References Departments (DepartID),
	ClassID nvarchar(155) Foreign Key (ClassID) References Classes (ClassID),
	StuMajor nvarchar(155) Foreign Key References Majors (MajorID),
	FirstName nvarchar(20),
	LastName nvarchar(50),
	PhoneNum nvarchar(20),
	PhoneEmer nvarchar(20),
	Mail nvarchar(155),
	DOB date,
	Address nvarchar(255),
	Image nvarchar(555),
	EnrollmentDate date
);
go

Create Table Subjects
(
	SubID nvarchar(155) Primary Key, -- UsingUUIDv7
	DepartID nvarchar(155) Foreign Key References Departments (DepartID),
	SubMajor nvarchar (155) Foreign Key References Majors (MajorID),
	SubName nvarchar(200),
	SubCode nvarchar(155),
	Description nvarchar(255)
);
go

--ClassRoom
Create table Classrooms
(
	ClassroomID nvarchar(155) Primary Key, --Using UUIDv7
	ClassName nvarchar (255),
	ClassSub nvarchar(155) Foreign Key References Subjects (SubID),
	MainClass nvarchar(155) Foreign Key References Classes(ClassID),
	Description nvarchar(555), 
	InviteCode nvarchar(15),
	CreateBy nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	ClassStatus nvarchar (25) --InTime, AllowEnd, NonAllowEnd
);
go

--Classroom Members in class
Create Table ClassroomMembers
(
	MemberID nvarchar(155) Primary key, --Uing UUIDv7
	ClassroomID nvarchar(155) Foreign Key References Classrooms (ClassroomID),
	LecturerID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	StudentID nvarchar(155) Foreign Key References Students (StudentID),
	RoleInClass nvarchar (15) -- Teacher or Student
);
go

Create Table Lessons
(
	LessonID nvarchar(155) Primary Key, --Using UUIDv7
	CreateBy nvarchar(155) Foreign Key (CreateBy) References DepartmentStaffs (StaffID),
	ClassroomID nvarchar(155) Foreign Key References Classrooms (ClassroomID),
	Title nvarchar(255),
	Content nvarchar(Max),
	CreateAt DateTime Default GetDate()
);
go

--1 Lession have n file
Create Table LessonFiles
(
	FilesID nvarchar(155) Primary Key, --Using UUIDv7
	LessonID nvarchar(155) Foreign Key (LessonID) References Lessons (LessonID),
	FileName nvarchar(255),
	FilePath nvarchar(555),
	FileType nvarchar(55),
	UpdateAt Datetime Default GetDate()
);
Go

--Homework (Assigments)
Create Table Assignments
(
	AssignID nvarchar(155) Primary Key, --Using UUIDv7
	ClassroomID nvarchar(155) Foreign Key References Classrooms (ClassroomID),
	TeacherID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	Title nvarchar(255),
	Description nvarchar(Max),
	Deadline DateTime,
	DeadlineStatus nvarchar(35), --Waiting, Overdue
	HomeworkStatus nvarchar(35), --AllowLate, NonLateAllow
	CreateAt DateTime Default GetDate()
);
Go

--Assignment File if 1 Assignment have n file
Create Table AssignmentFiles
(
	FileID nvarchar(155) Primary Key,
	AssignID nvarchar(155) Foreign Key References Assignments (AssignID),
	FileName nvarchar(255),
	FilePath nvarchar(555),
	FileType nvarchar(55)
);
go

--Submit Assignment
Create Table Submissions
(
	SubmitID nvarchar(155) Primary Key, --Using UUIDv7
	AssignID nvarchar(155) Foreign Key References Assignments (AssignID),
	StudentID nvarchar(155) Foreign Key (StudentID) References Students (StudentID),
	SubmitAt DateTime Default GetDate(),
	FeedBack nvarchar(255),
	SubmitType nvarchar(55),
	Grade float
);
Go

--If 1 submit n file 
Create Table SubmitFiles
(
	FileID nvarchar(155) Primary Key, --Using UUIDv7
	SubmitID nvarchar(155) Foreign Key References Submissions (SubmitID),
	FileName nvarchar (255),
	FilePath nvarchar(555),
	FileType nvarchar(50),
	UpdateAt DateTime Default GetDate()
);
Go

Create Table Attendances
(
	AttendanceID nvarchar(155) Primary Key,
	StudentID nvarchar(155) Foreign Key References  Students (StudentID),
	ClassroomID nvarchar(155) Foreign Key References Classrooms (ClassroomID),
	LecturerID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	Status Nvarchar(25), --P/A/AP
	CheckInTime DateTime2 Default GetDate(),
	StartTime DateTime2 Default GetDate(),
	EndTime DateTime2
);
Go

Create Table ActivityLogs
(
	LogID nvarchar(155) Primary Key, --Using UUIDv7
	StaffID nvarchar(155) Foreign Key (StaffID) References DepartmentStaffs (StaffID),
	DepartID nvarchar(155) Foreign Key References Departments (DepartID),
	Action nvarchar(555),
	TargetTable nvarchar(155),
	TargetID nvarchar(155),
	TargetName nvarchar(255),
	Timestap DateTime Default GetDate(),
);
Go

Create Table Projects
(
	ProjectID nvarchar(155) Primary Key,
	Title Nvarchar (255),
	ProMajor nvarchar(155) Foreign Key References Majors (MajorID),
	Cohort nvarchar(20),
	MaxStudents int, 
	Description Nvarchar(MAX),
	StartDate DateTime,
	EndDate DateTime,
	CreateBy nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	CreateDate Datetime2 Default GetDate()
);
Go

Create Table ProjectAssigns
(
	AssignID nvarchar(155) Primary Key,
	ProjectID nvarchar(155) Foreign Key References Projects (ProjectID),
	SpecialID nvarchar(155) Foreign Key References Specializations (SpecialID),
	GroupName nvarchar(100),
	Description nvarchar(255),
	LecturerID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	CreateBy nvarchar(155) Foreign Key References Students (StudentID),
	CreateDate DateTime Default GetDate(),
	Status Nvarchar(55),
	ApprovalNote nvarchar(max)
);
go

Create Table ProjectMenbers
(
	MemberID nvarchar(155) Primary Key,
	RegistID nvarchar(155) Foreign Key References ProjectAssigns (AssignID),
	StudentID nvarchar(155) Foreign Key References Students (StudentID)
);
Go

Create Table ProjectApprovals
(
	ApprovalID nvarchar(155) Primary Key,
	RegistID nvarchar(155) Foreign Key References ProjectAssigns (AssignID),
	ApproverID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	Decision nvarchar(20),
	Comments nvarchar(Max),
	ApprovalDate DateTime2 Default GetDate()
);
Go

Create Table ProjectClassrooms
(
	ProClassID nvarchar(155) Primary Key,
	ProjectID nvarchar(155) Foreign Key References Projects (ProjectID),
	ClassroomName nvarchar(255),
	CreateDate DateTime2 Default GetDate(),
	IsActive BIT
);
go

Create Table ProjectClassMems
(
	ProClassMemID nvarchar(155) Primary Key,
	ProClassID nvarchar(155) Foreign Key References ProjectClassrooms (ProClassID),
	LecturerID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	AssignID nvarchar(155) Foreign Key References ProjectAssigns (AssignID),
	RoleInClass nvarchar(15) 
);
go

Create Table WeeklyMeetings
(
	MeetingID nvarchar(155)Primary Key,
	ProClassID nvarchar(155) Foreign Key References ProjectClassrooms (ProClassID),
	LecturerID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	MeetingDate Datetime2,
	Notes nvarchar(max)
);
Go

Create Table WeeklyMeetAttendences
(
	MeetAttendenceID nvarchar(155) Primary Key,
	Status nvarchar(25),
	MeetingID nvarchar(155) Foreign Key References WeeklyMeetings (MeetingID)
);
Go

Create Table LecturerWeeklyReports
(
	LecReportID nvarchar(155) Primary Key,
	AssigntID nvarchar(155) Foreign Key References ProjectAssigns (AssignID),
	ReportWritter nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	WeeklyMeeting nvarchar(155) Foreign Key References WeeklyMeetings (MeetingID),
	WeekNumber int,
	ReportContent Nvarchar(max),
	SubmitDate Datetime2 Default Getdate()
);
Go

Create Table StudentWeeklyReports
(
	StuReportID nvarchar(155) Primary Key,
	AssigntID nvarchar(155) Foreign Key References ProjectAssigns (AssignID),
	ReportWritter nvarchar(155) Foreign Key References Students (StudentID),
	WeekNumber int,
	ReportContent Nvarchar(max),
	SubmitDate Datetime2 Default Getdate()
);
go

Create Table ProjectContents
(
	ProContentID nvarchar(155) Primary Key,
	ProClassID nvarchar(155) Foreign Key References ProjectClassrooms (ProClassID),
	PostedBy nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	Title nvarchar(255),
	ContentText Nvarchar(Max),
	Deadline DateTime2 Null, --If this Type Is Assigment
	CreateDate DateTime2 Default GetDate(),
	ContentType Nvarchar(55)
);
Go

Create Table ProConFiles
(
	ProConFileID nvarchar(155) Primary Key,
	ProContentID nvarchar(155) Foreign Key References ProjectContents (ProContentID),
	FileName nvarchar(255),
	FilePath nvarchar(555),
	FileType nvarchar(100)
);
Go

Create Table ProjectSubmissions
(
	ProSubmitID nvarchar(155) Primary Key,
	ProContentID nvarchar(155) Foreign Key References ProjectContents (ProContentID),
	StudentID nvarchar(155) Foreign Key References Students (StudentID),
	SubmitDate DateTime2 Default GetDate(),
	Score Float null,
	FeedBack Nvarchar(Max) Null
);
Go

Create Table ProjectSubmitFiles
(
	ProSubmitFiles Nvarchar(155) Primary Key,
	ProSubmitID nvarchar(155) Foreign Key References ProjectSubmissions (ProSubmitID),
	FileName nvarchar(255),
	FilePath nvarchar(555),
	FileType nvarchar(25)
);
Go

Create Table Internships
(
	InternID nvarchar(155) Primary Key,
	Title Nvarchar(255),
	MajorID nvarchar(155) Foreign Key References Majors (MajorID),
	Cohort nvarchar(25),
	AvailForm nvarchar(25),
	Description nvarchar(MAX),
	MaxStudent int,
	StartDate DateTime,
	EndDate DateTime,
	CreateBy nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	CreateDate DateTime2 Default GetDate()
);
Go

Create Table InternRegists
(
	RegistID nvarchar(155) Primary Key,
	IntershipID nvarchar(155) Foreign Key References Internships (InternID),
	StudentID nvarchar(155) Foreign Key References Students (StudentID),
	HasCompany BIT,
	RequestLetter BIT,
	Status nvarchar(50),
	ApprovalNote Nvarchar(Max) Null,
	CreateDate DateTime2 Default GetDate()
);
Go

Create Table InternshipCompanyInfors
(
	CompanyInfoID nvarchar(155) Primary Key,
	RegisID nvarchar(155) Foreign Key References InternRegists (RegistID),
	CompanyName nvarchar(355),
	Address nvarchar(255),
	HRRCompanyPhone nvarchar(20),
	HRRCompanyEmail nvarchar(100),
	ContactPerson nvarchar(100),
	ContactPhone Nvarchar(20),
	ContactEmail Nvarchar(155),
	ContactEmployeeCode nvarchar(155)
);
go

Create Table InternAppovals
(
	ApprovalID nvarchar(155) Primary Key,
	RegistID nvarchar(155) Foreign Key References InternRegists (RegistID),
	ApproverID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	Decision nvarchar(20),
	Comments Nvarchar(Max),
	ApporvalDate DateTime2 Default GetDate()
);
GO

Create Table InternAssignments
(
	AssignID nvarchar(155) Primary Key,
	RegistID nvarchar(155) Foreign Key References InternRegists (RegistID),
	LecturerID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID)
);
Go

Create Table InternClassrooms
(
	InternClassID nvarchar(155) Primary Key,
	LecturerID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	InternID nvarchar(155) Foreign Key References Internships (InternID),
	ClassroomName Nvarchar(255),
	Description nvarchar(355),
	CreateDate DateTime2 Default GetDate(),
);
Go

Create Table InternClassMems
(
	MemberID nvarchar(155) Primary Key,
	InternClassID Nvarchar(155) Foreign Key References InternClassrooms (InternClassID),
	AssignID nvarchar(155) Foreign Key References InternAssignments (AssignID)
);
Go

Create Table InternContents
(
	InternContentID Nvarchar(155) Primary Key,
	InternClassID nvarchar(155) Foreign Key References InternClassrooms (InternClassID),
	PostedBy Nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	Title nvarchar(255),
	ContentText Nvarchar(Max),
	Deadline DateTime2 Default GetDate(),
	ContentType Nvarchar(50)
);
Go

Create Table InConFiles
(
	InConFileID nvarchar(155) Primary Key,
	InternContentID nvarchar(155) Foreign Key References InternContents (InternContentID),
	FileName nvarchar (255),
	FilePaht Nvarchar(355),
	FileType Nvarchar(25)
);
GO

Create Table InternSubmissions
(
	SubmitID nvarchar(155) Primary Key,
	InternContentID nvarchar(155) Foreign Key References InternContents (InternContentID),
	StudentID nvarchar(155) Foreign Key References Students (StudentID),
	SubmitDate DateTime2 Default GetDate(),
	Feedback nvarchar(max)
);
Go

Create Table StuInternWeeklyReports
(
	StuReprotID nvarchar(155) Primary Key,
	AssignID nvarchar(155) Foreign Key References InternAssignments (AssignID),
	StudentID nvarchar(155) Foreign Key References Students (StudentID),
	WeekNumber int,
	ReportContent Nvarchar(MAX),
	SubmitDate DateTime2 Default GetDate()
);
Go

Create Table LecturerInternReports
(
	LecReportID nvarchar(155) Primary Key,
	AssignID nvarchar(155) Foreign Key References InternAssignments (AssignID),
	LecturerID nvarchar(155) Foreign Key References DepartmentStaffs (StaffID),
	WeekNumber int,
	ReportContent nvarchar(max),
	SubmitDate Datetime2 Default GetDate()
);
Go