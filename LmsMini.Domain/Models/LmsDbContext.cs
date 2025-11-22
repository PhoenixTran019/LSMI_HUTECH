using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace LmsMini.Domain.Models;

public partial class LmsDbContext : DbContext
{
    public LmsDbContext()
    {
    }

    public LmsDbContext(DbContextOptions<LmsDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<ActivityLog> ActivityLogs { get; set; }

    public virtual DbSet<Assignment> Assignments { get; set; }

    public virtual DbSet<AssignmentFile> AssignmentFiles { get; set; }

    public virtual DbSet<Attendance> Attendances { get; set; }

    public virtual DbSet<Class> Classes { get; set; }

    public virtual DbSet<Classroom> Classrooms { get; set; }

    public virtual DbSet<ClassroomMember> ClassroomMembers { get; set; }

    public virtual DbSet<Department> Departments { get; set; }

    public virtual DbSet<DepartmentStaff> DepartmentStaffs { get; set; }

    public virtual DbSet<InConFile> InConFiles { get; set; }

    public virtual DbSet<InternAppoval> InternAppovals { get; set; }

    public virtual DbSet<InternAssignment> InternAssignments { get; set; }

    public virtual DbSet<InternClassMem> InternClassMems { get; set; }

    public virtual DbSet<InternClassroom> InternClassrooms { get; set; }

    public virtual DbSet<InternContent> InternContents { get; set; }

    public virtual DbSet<InternRegist> InternRegists { get; set; }

    public virtual DbSet<InternSubmission> InternSubmissions { get; set; }

    public virtual DbSet<Internship> Internships { get; set; }

    public virtual DbSet<InternshipCompanyInfor> InternshipCompanyInfors { get; set; }

    public virtual DbSet<LecturerInternReport> LecturerInternReports { get; set; }

    public virtual DbSet<LecturerWeeklyReport> LecturerWeeklyReports { get; set; }

    public virtual DbSet<Lesson> Lessons { get; set; }

    public virtual DbSet<LessonFile> LessonFiles { get; set; }

    public virtual DbSet<Major> Majors { get; set; }

    public virtual DbSet<ProConFile> ProConFiles { get; set; }

    public virtual DbSet<Project> Projects { get; set; }

    public virtual DbSet<ProjectApproval> ProjectApprovals { get; set; }

    public virtual DbSet<ProjectAssign> ProjectAssigns { get; set; }

    public virtual DbSet<ProjectClassMem> ProjectClassMems { get; set; }

    public virtual DbSet<ProjectClassroom> ProjectClassrooms { get; set; }

    public virtual DbSet<ProjectContent> ProjectContents { get; set; }

    public virtual DbSet<ProjectMenber> ProjectMenbers { get; set; }

    public virtual DbSet<ProjectSubmission> ProjectSubmissions { get; set; }

    public virtual DbSet<ProjectSubmitFile> ProjectSubmitFiles { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<SchoolYear> SchoolYears { get; set; }

    public virtual DbSet<Semester> Semesters { get; set; }

    public virtual DbSet<Specialization> Specializations { get; set; }
    public virtual DbSet<StaffDepart> StaffDeparts { get; set; }

    public virtual DbSet<StuInternWeeklyReport> StuInternWeeklyReports { get; set; }

    public virtual DbSet<Student> Students { get; set; }

    public virtual DbSet<StudentWeeklyReport> StudentWeeklyReports { get; set; }

    public virtual DbSet<Subject> Subjects { get; set; }

    public virtual DbSet<Submission> Submissions { get; set; }

    public virtual DbSet<SubmitFile> SubmitFiles { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<WeeklyMeetAttendence> WeeklyMeetAttendences { get; set; }

    public virtual DbSet<WeeklyMeeting> WeeklyMeetings { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
//#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=DbLMS;Trusted_Connection=True;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.HasKey(e => e.LogId).HasName("PK__Activity__5E5499A895097F8C");

            entity.Property(e => e.LogId)
                .HasMaxLength(155)
                .HasColumnName("LogID");
            entity.Property(e => e.Action).HasMaxLength(555);
            entity.Property(e => e.DepartId)
                .HasMaxLength(155)
                .HasColumnName("DepartID");
            entity.Property(e => e.StaffId)
                .HasMaxLength(155)
                .HasColumnName("StaffID");
            entity.Property(e => e.TargetId)
                .HasMaxLength(155)
                .HasColumnName("TargetID");
            entity.Property(e => e.TargetName).HasMaxLength(255);
            entity.Property(e => e.TargetTable).HasMaxLength(155);
            entity.Property(e => e.Timestap)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Depart).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.DepartId)
                .HasConstraintName("FK__ActivityL__Depar__08B54D69");

            entity.HasOne(d => d.Staff).WithMany(p => p.ActivityLogs)
                .HasForeignKey(d => d.StaffId)
                .HasConstraintName("FK__ActivityL__Staff__07C12930");
        });

        modelBuilder.Entity<Assignment>(entity =>
        {
            entity.HasKey(e => e.AssignId).HasName("PK__Assignme__9FFF4C4F5836E511");

            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.ClassroomId)
                .HasMaxLength(155)
                .HasColumnName("ClassroomID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Deadline).HasColumnType("datetime");
            entity.Property(e => e.DeadlineStatus).HasMaxLength(35);
            entity.Property(e => e.HomeworkStatus).HasMaxLength(35);
            entity.Property(e => e.TeacherId)
                .HasMaxLength(155)
                .HasColumnName("TeacherID");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Classroom).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.ClassroomId)
                .HasConstraintName("FK__Assignmen__Class__71D1E811");

            entity.HasOne(d => d.Teacher).WithMany(p => p.Assignments)
                .HasForeignKey(d => d.TeacherId)
                .HasConstraintName("FK__Assignmen__Teach__72C60C4A");
        });

        modelBuilder.Entity<AssignmentFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__Assignme__6F0F989F5C2DD689");

            entity.Property(e => e.FileId)
                .HasMaxLength(155)
                .HasColumnName("FileID");
            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(555);
            entity.Property(e => e.FileType).HasMaxLength(55);

            entity.HasOne(d => d.Assign).WithMany(p => p.AssignmentFiles)
                .HasForeignKey(d => d.AssignId)
                .HasConstraintName("FK__Assignmen__Assig__76969D2E");
        });

        modelBuilder.Entity<Attendance>(entity =>
        {
            entity.HasKey(e => e.AttendanceId).HasName("PK__Attendan__8B69263CBA177121");

            entity.Property(e => e.AttendanceId)
                .HasMaxLength(155)
                .HasColumnName("AttendanceID");
            entity.Property(e => e.CheckInTime).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.StartTime).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(155)
                .HasColumnName("LecturerID");
            entity.Property(e => e.ClassroomId)
                .HasMaxLength(155)
                .HasColumnName("ClassroomID");
            entity.Property(e => e.Status).HasMaxLength(25);
            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__Attendanc__Lectu__03F0984C");

            entity.HasOne(d => d.Classroom).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.ClassroomId)
                .HasConstraintName("FK__Attendanc__Class__02FC7412");

            entity.HasOne(d => d.Student).WithMany(p => p.Attendances)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Attendanc__Stude__02FC7413");
        });

        modelBuilder.Entity<Class>(entity =>
        {
            entity.HasKey(e => e.ClassId).HasName("PK__Classes__CB1927A0B807F4C8");

            entity.Property(e => e.ClassId)
                .HasMaxLength(155)
                .HasColumnName("ClassID");
            entity.Property(e => e.ClassMajor).HasMaxLength(155);
            entity.Property(e => e.ClassName).HasMaxLength(50);
            entity.Property(e => e.Course).HasMaxLength(15);
            entity.Property(e => e.DepartId)
                .HasMaxLength(155)
                .HasColumnName("DepartID");

            entity.HasOne(d => d.ClassMajorNavigation).WithMany(p => p.Classes)
                .HasForeignKey(d => d.ClassMajor)
                .HasConstraintName("FK__Classes__ClassMa__52593CB8");

            entity.HasOne(d => d.Depart).WithMany(p => p.Classes)
                .HasForeignKey(d => d.DepartId)
                .HasConstraintName("FK_Classes_Departments");
        });

        modelBuilder.Entity<Classroom>(entity =>
        {
            entity.HasKey(e => e.ClassroomId).HasName("PK__Classroo__11618E8A49898622");

            entity.Property(e => e.ClassroomId)
                .HasMaxLength(155)
                .HasColumnName("ClassroomID");
            entity.Property(e => e.ClassName).HasMaxLength(255);
            entity.Property(e => e.ClassStatus).HasMaxLength(25);
            entity.Property(e => e.ClassSub).HasMaxLength(155);
            entity.Property(e => e.CreateBy).HasMaxLength(155);
            entity.Property(e => e.Description).HasMaxLength(555);
            entity.Property(e => e.InviteCode).HasMaxLength(15);
            entity.Property(e => e.MainClass).HasMaxLength(155);

            entity.HasOne(d => d.ClassSubNavigation).WithMany(p => p.Classrooms)
                .HasForeignKey(d => d.ClassSub)
                .HasConstraintName("FK__Classroom__Class__5EBF139D");

            entity.HasOne(d => d.CreateByNavigation).WithMany(p => p.Classrooms)
                .HasForeignKey(d => d.CreateBy)
                .HasConstraintName("FK__Classroom__Creat__5FB337D6");

            entity.HasOne(d => d.MainClassNavigation).WithMany(p => p.Classrooms)
                .HasForeignKey(d => d.MainClass)
                .HasConstraintName("FK_Classrooms_MainClass");
        });

        modelBuilder.Entity<ClassroomMember>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__Classroo__0CF04B38F557CECA");

            entity.Property(e => e.MemberId)
                .HasMaxLength(155)
                .HasColumnName("MemberID");
            entity.Property(e => e.ClassroomId)
                .HasMaxLength(155)
                .HasColumnName("ClassroomID");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(155)
                .HasColumnName("LecturerID");
            entity.Property(e => e.RoleInClass).HasMaxLength(15);
            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");

            entity.HasOne(d => d.Classroom).WithMany(p => p.ClassroomMembers)
                .HasForeignKey(d => d.ClassroomId)
                .HasConstraintName("FK__Classroom__Class__628FA481");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.ClassroomMembers)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__Classroom__Lectu__6383C8BA");

            entity.HasOne(d => d.Student).WithMany(p => p.ClassroomMembers)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Classroom__Stude__6477ECF3");
        });

        modelBuilder.Entity<Department>(entity =>
        {
            entity.HasKey(e => e.DepartId).HasName("PK__Departme__0E53E1D52B4648A3");

            entity.Property(e => e.DepartId)
                .HasMaxLength(155)
                .HasColumnName("DepartID");
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.ApprovalDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.DepartDescription).HasMaxLength(155);
            entity.Property(e => e.DepartName).HasMaxLength(155);
            entity.Property(e => e.Image).HasMaxLength(555);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Status).HasMaxLength(55);
        });

        modelBuilder.Entity<DepartmentStaff>(entity =>
        {
            entity.HasKey(e => e.StaffId).HasName("PK__Departme__96D4AAF731DC9395");

            entity.Property(e => e.StaffId)
                .HasMaxLength(155)
                .HasColumnName("StaffID");
            entity.Property(e => e.Address).HasMaxLength(300);
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.FirstName).HasMaxLength(20);
            entity.Property(e => e.HireDate).HasColumnType("datetime");
            entity.Property(e => e.Image).HasMaxLength(555);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Mail).HasMaxLength(155);
            entity.Property(e => e.Majors).HasMaxLength(155);
            entity.Property(e => e.PhoneNum).HasMaxLength(20);
            entity.Property(e => e.StaffRole).HasMaxLength(155);
            entity.Property(e => e.Status).HasMaxLength(100);
            entity.Property(e => e.UserId)
                .HasMaxLength(155)
                .HasColumnName("UserID");

            entity.HasOne(d => d.MajorsNavigation).WithMany(p => p.DepartmentStaffs)
                .HasForeignKey(d => d.Majors)
                .HasConstraintName("FK__Departmen__Major__4E88ABD4");

            entity.HasOne(d => d.StaffRoleNavigation).WithMany(p => p.DepartmentStaffs)
                .HasForeignKey(d => d.StaffRole)
                .HasConstraintName("FK_Department_Role");

            entity.HasOne(d => d.User).WithMany(p => p.DepartmentStaffs)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Departmen__UserI__4CA06362");

            
        });

        modelBuilder.Entity<InConFile>(entity =>
        {
            entity.HasKey(e => e.InConFileId).HasName("PK__InConFil__9F6C553CE659D858");

            entity.Property(e => e.InConFileId)
                .HasMaxLength(155)
                .HasColumnName("InConFileID");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePaht).HasMaxLength(355);
            entity.Property(e => e.FileType).HasMaxLength(25);
            entity.Property(e => e.InternContentId)
                .HasMaxLength(155)
                .HasColumnName("InternContentID");

            entity.HasOne(d => d.InternContent).WithMany(p => p.InConFiles)
                .HasForeignKey(d => d.InternContentId)
                .HasConstraintName("FK__InConFile__Inter__6BE40491");
        });

        modelBuilder.Entity<InternAppoval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId).HasName("PK__InternAp__328477D414FC1E03");

            entity.Property(e => e.ApprovalId)
                .HasMaxLength(155)
                .HasColumnName("ApprovalID");
            entity.Property(e => e.ApporvalDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ApproverId)
                .HasMaxLength(155)
                .HasColumnName("ApproverID");
            entity.Property(e => e.Decision).HasMaxLength(20);
            entity.Property(e => e.RegistId)
                .HasMaxLength(155)
                .HasColumnName("RegistID");

            entity.HasOne(d => d.Approver).WithMany(p => p.InternAppovals)
                .HasForeignKey(d => d.ApproverId)
                .HasConstraintName("FK__InternApp__Appro__56E8E7AB");

            entity.HasOne(d => d.Regist).WithMany(p => p.InternAppovals)
                .HasForeignKey(d => d.RegistId)
                .HasConstraintName("FK__InternApp__Regis__55F4C372");
        });

        modelBuilder.Entity<InternAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignId).HasName("PK__InternAs__9FFF4C4F01904825");

            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(155)
                .HasColumnName("LecturerID");
            entity.Property(e => e.RegistId)
                .HasMaxLength(155)
                .HasColumnName("RegistID");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.InternAssignments)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__InternAss__Lectu__5BAD9CC8");

            entity.HasOne(d => d.Regist).WithMany(p => p.InternAssignments)
                .HasForeignKey(d => d.RegistId)
                .HasConstraintName("FK__InternAss__Regis__5AB9788F");
        });

        modelBuilder.Entity<InternClassMem>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__InternCl__0CF04B3833354D8A");

            entity.Property(e => e.MemberId)
                .HasMaxLength(155)
                .HasColumnName("MemberID");
            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.InternClassId)
                .HasMaxLength(155)
                .HasColumnName("InternClassID");

            entity.HasOne(d => d.Assign).WithMany(p => p.InternClassMems)
                .HasForeignKey(d => d.AssignId)
                .HasConstraintName("FK__InternCla__Assig__6442E2C9");

            entity.HasOne(d => d.InternClass).WithMany(p => p.InternClassMems)
                .HasForeignKey(d => d.InternClassId)
                .HasConstraintName("FK__InternCla__Inter__634EBE90");
        });

        modelBuilder.Entity<InternClassroom>(entity =>
        {
            entity.HasKey(e => e.InternClassId).HasName("PK__InternCl__9B61AE26E3494D42");

            entity.Property(e => e.InternClassId)
                .HasMaxLength(155)
                .HasColumnName("InternClassID");
            entity.Property(e => e.ClassroomName).HasMaxLength(255);
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.Description).HasMaxLength(355);
            entity.Property(e => e.InviteCode).HasMaxLength(15);
            entity.Property(e => e.InternId)
                .HasMaxLength(155)
                .HasColumnName("InternID");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(155)
                .HasColumnName("LecturerID");

            entity.HasOne(d => d.Intern).WithMany(p => p.InternClassrooms)
                .HasForeignKey(d => d.InternId)
                .HasConstraintName("FK__InternCla__Inter__5F7E2DAC");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.InternClassrooms)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__InternCla__Lectu__5E8A0973");
        });

        modelBuilder.Entity<InternContent>(entity =>
        {
            entity.HasKey(e => e.InternContentId).HasName("PK__InternCo__C133293EE8EA2FD7");

            entity.Property(e => e.InternContentId)
                .HasMaxLength(155)
                .HasColumnName("InternContentID");
            entity.Property(e => e.ContentType).HasMaxLength(50);
            entity.Property(e => e.Deadline).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.InternClassId)
                .HasMaxLength(155)
                .HasColumnName("InternClassID");
            entity.Property(e => e.PostedBy).HasMaxLength(155);
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.InternClass).WithMany(p => p.InternContents)
                .HasForeignKey(d => d.InternClassId)
                .HasConstraintName("FK__InternCon__Inter__671F4F74");

            entity.HasOne(d => d.PostedByNavigation).WithMany(p => p.InternContents)
                .HasForeignKey(d => d.PostedBy)
                .HasConstraintName("FK__InternCon__Poste__681373AD");
        });

        modelBuilder.Entity<InternRegist>(entity =>
        {
            entity.HasKey(e => e.RegistId).HasName("PK__InternRe__8C2083BE6D71A1E6");

            entity.Property(e => e.RegistId)
                .HasMaxLength(155)
                .HasColumnName("RegistID");
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IntershipId)
                .HasMaxLength(155)
                .HasColumnName("IntershipID");
            entity.Property(e => e.Status).HasMaxLength(50);
            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");

            entity.HasOne(d => d.Intership).WithMany(p => p.InternRegists)
                .HasForeignKey(d => d.IntershipId)
                .HasConstraintName("FK__InternReg__Inter__4E53A1AA");

            entity.HasOne(d => d.Student).WithMany(p => p.InternRegists)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__InternReg__Stude__4F47C5E3");
        });

        modelBuilder.Entity<InternSubmission>(entity =>
        {
            entity.HasKey(e => e.SubmitId).HasName("PK__InternSu__421A8E1E82FDC3A2");

            entity.Property(e => e.SubmitId)
                .HasMaxLength(155)
                .HasColumnName("SubmitID");
            entity.Property(e => e.InternContentId)
                .HasMaxLength(155)
                .HasColumnName("InternContentID");
            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");
            entity.Property(e => e.SubmitDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.InternContent).WithMany(p => p.InternSubmissions)
                .HasForeignKey(d => d.InternContentId)
                .HasConstraintName("FK__InternSub__Inter__6EC0713C");

            entity.HasOne(d => d.Student).WithMany(p => p.InternSubmissions)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__InternSub__Stude__6FB49575");
        });

        modelBuilder.Entity<Internship>(entity =>
        {
            entity.HasKey(e => e.InternId).HasName("PK__Internsh__6910ED82578F03D6");

            entity.Property(e => e.InternId)
                .HasMaxLength(155)
                .HasColumnName("InternID");
            entity.Property(e => e.AvailForm).HasMaxLength(25);
            entity.Property(e => e.Cohort).HasMaxLength(25);
            entity.Property(e => e.CreateBy).HasMaxLength(155);
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.MajorId)
                .HasMaxLength(155)
                .HasColumnName("MajorID");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.CreateByNavigation).WithMany(p => p.Internships)
                .HasForeignKey(d => d.CreateBy)
                .HasConstraintName("FK__Internshi__Creat__4A8310C6");

            entity.HasOne(d => d.Major).WithMany(p => p.Internships)
                .HasForeignKey(d => d.MajorId)
                .HasConstraintName("FK__Internshi__Major__498EEC8D");
        });

        modelBuilder.Entity<InternshipCompanyInfor>(entity =>
        {
            entity.HasKey(e => e.CompanyInfoId).HasName("PK__Internsh__AA677752B3A96641");

            entity.Property(e => e.CompanyInfoId)
                .HasMaxLength(155)
                .HasColumnName("CompanyInfoID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.CompanyName).HasMaxLength(355);
            entity.Property(e => e.ContactEmail).HasMaxLength(155);
            entity.Property(e => e.ContactEmployeeCode).HasMaxLength(155);
            entity.Property(e => e.ContactPerson).HasMaxLength(100);
            entity.Property(e => e.ContactPhone).HasMaxLength(20);
            entity.Property(e => e.HrrcompanyEmail)
                .HasMaxLength(100)
                .HasColumnName("HRRCompanyEmail");
            entity.Property(e => e.HrrcompanyPhone)
                .HasMaxLength(20)
                .HasColumnName("HRRCompanyPhone");
            entity.Property(e => e.RegisId)
                .HasMaxLength(155)
                .HasColumnName("RegisID");

            entity.HasOne(d => d.Regis).WithMany(p => p.InternshipCompanyInfors)
                .HasForeignKey(d => d.RegisId)
                .HasConstraintName("FK__Internshi__Regis__531856C7");
        });

        modelBuilder.Entity<LecturerInternReport>(entity =>
        {
            entity.HasKey(e => e.LecReportId).HasName("PK__Lecturer__F29AA0FBC57D31A5");

            entity.Property(e => e.LecReportId)
                .HasMaxLength(155)
                .HasColumnName("LecReportID");
            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(155)
                .HasColumnName("LecturerID");
            entity.Property(e => e.SubmitDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Assign).WithMany(p => p.LecturerInternReports)
                .HasForeignKey(d => d.AssignId)
                .HasConstraintName("FK__LecturerI__Assig__7849DB76");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.LecturerInternReports)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__LecturerI__Lectu__793DFFAF");
        });

        modelBuilder.Entity<LecturerWeeklyReport>(entity =>
        {
            entity.HasKey(e => e.LecReportId).HasName("PK__Lecturer__F29AA0FB9214FE7E");

            entity.Property(e => e.LecReportId)
                .HasMaxLength(155)
                .HasColumnName("LecReportID");
            entity.Property(e => e.AssigntId)
                .HasMaxLength(155)
                .HasColumnName("AssigntID");
            entity.Property(e => e.ReportWritter).HasMaxLength(155);
            entity.Property(e => e.SubmitDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.WeeklyMeeting).HasMaxLength(155);

            entity.HasOne(d => d.Assignt).WithMany(p => p.LecturerWeeklyReports)
                .HasForeignKey(d => d.AssigntId)
                .HasConstraintName("FK__LecturerW__Assig__2FCF1A8A");

            entity.HasOne(d => d.ReportWritterNavigation).WithMany(p => p.LecturerWeeklyReports)
                .HasForeignKey(d => d.ReportWritter)
                .HasConstraintName("FK__LecturerW__Repor__30C33EC3");

            entity.HasOne(d => d.WeeklyMeetingNavigation).WithMany(p => p.LecturerWeeklyReports)
                .HasForeignKey(d => d.WeeklyMeeting)
                .HasConstraintName("FK__LecturerW__Weekl__31B762FC");
        });

        modelBuilder.Entity<Lesson>(entity =>
        {
            entity.HasKey(e => e.LessonId).HasName("PK__Lessons__B084ACB067FB621E");

            entity.Property(e => e.LessonId)
                .HasMaxLength(155)
                .HasColumnName("LessonID");
            entity.Property(e => e.ClassroomId)
                .HasMaxLength(155)
                .HasColumnName("ClassroomID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.CreateBy).HasMaxLength(155);
           
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.Classroom).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.ClassroomId)
                .HasConstraintName("FK__Lessons__Classro__6A30C649");

            entity.HasOne(d => d.CreateByNavigation).WithMany(p => p.Lessons)
                .HasForeignKey(d => d.CreateBy)
                .HasConstraintName("FK__Lessons__CreateB__68487DD7");
        });

        modelBuilder.Entity<LessonFile>(entity =>
        {
            entity.HasKey(e => e.FilesId).HasName("PK__LessonFi__000BC268AA323D6D");

            entity.Property(e => e.FilesId)
                .HasMaxLength(155)
                .HasColumnName("FilesID");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(555);
            entity.Property(e => e.FileType).HasMaxLength(55);
            entity.Property(e => e.LessonId)
                .HasMaxLength(155)
                .HasColumnName("LessonID");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Lesson).WithMany(p => p.LessonFiles)
                .HasForeignKey(d => d.LessonId)
                .HasConstraintName("FK__LessonFil__Lesso__6E01572D");
        });

        modelBuilder.Entity<Major>(entity =>
        {
            entity.HasKey(e => e.MajorId).HasName("PK__Majors__D5B8BFB10AAE8507");

            entity.Property(e => e.MajorId)
                .HasMaxLength(155)
                .HasColumnName("MajorID");
            entity.Property(e => e.MajorDescription).HasMaxLength(155);
            entity.Property(e => e.MajorName).HasMaxLength(155);
        });

        modelBuilder.Entity<ProConFile>(entity =>
        {
            entity.HasKey(e => e.ProConFileId).HasName("PK__ProConFi__3EA8F03CB2ED80F2");

            entity.Property(e => e.ProConFileId)
                .HasMaxLength(155)
                .HasColumnName("ProConFileID");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(555);
            entity.Property(e => e.FileType).HasMaxLength(100);
            entity.Property(e => e.ProContentId)
                .HasMaxLength(155)
                .HasColumnName("ProContentID");

            entity.HasOne(d => d.ProContent).WithMany(p => p.ProConFiles)
                .HasForeignKey(d => d.ProContentId)
                .HasConstraintName("FK__ProConFil__ProCo__3F115E1A");
        });

        modelBuilder.Entity<Project>(entity =>
        {
            entity.HasKey(e => e.ProjectId).HasName("PK__Projects__761ABED0F42A14D4");

            entity.Property(e => e.ProjectId)
                .HasMaxLength(155)
                .HasColumnName("ProjectID");
            entity.Property(e => e.Cohort).HasMaxLength(20);
            entity.Property(e => e.CreateBy).HasMaxLength(155);
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.ProMajor).HasMaxLength(155);
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.CreateByNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.CreateBy)
                .HasConstraintName("FK__Projects__Create__0D7A0286");

            entity.HasOne(d => d.ProMajorNavigation).WithMany(p => p.Projects)
                .HasForeignKey(d => d.ProMajor)
                .HasConstraintName("FK__Projects__ProMaj__0C85DE4D");
        });

        modelBuilder.Entity<ProjectApproval>(entity =>
        {
            entity.HasKey(e => e.ApprovalId).HasName("PK__ProjectA__328477D4C146C98D");

            entity.Property(e => e.ApprovalId)
                .HasMaxLength(155)
                .HasColumnName("ApprovalID");
            entity.Property(e => e.ApprovalDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ApproverId)
                .HasMaxLength(155)
                .HasColumnName("ApproverID");
            entity.Property(e => e.Decision).HasMaxLength(20);
            entity.Property(e => e.RegistId)
                .HasMaxLength(155)
                .HasColumnName("RegistID");

            entity.HasOne(d => d.Approver).WithMany(p => p.ProjectApprovals)
                .HasForeignKey(d => d.ApproverId)
                .HasConstraintName("FK__ProjectAp__Appro__1CBC4616");

            entity.HasOne(d => d.Regist).WithMany(p => p.ProjectApprovals)
                .HasForeignKey(d => d.RegistId)
                .HasConstraintName("FK__ProjectAp__Regis__1BC821DD");
        });

        modelBuilder.Entity<ProjectAssign>(entity =>
        {
            entity.HasKey(e => e.AssignId).HasName("PK__ProjectA__9FFF4C4FF74FF9CF");

            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.CreateBy).HasMaxLength(155);
            entity.Property(e => e.CreateDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.GroupName).HasMaxLength(100);
            entity.Property(e => e.LecturerId)
                .HasMaxLength(155)
                .HasColumnName("LecturerID");
            entity.Property(e => e.ProjectId)
                .HasMaxLength(155)
                .HasColumnName("ProjectID");
            entity.Property(e => e.SpecialId)
                .HasMaxLength(155)
                .HasColumnName("SpecialID");
            entity.Property(e => e.Status).HasMaxLength(55);

            entity.HasOne(d => d.CreateByNavigation).WithMany(p => p.ProjectAssigns)
                .HasForeignKey(d => d.CreateBy)
                .HasConstraintName("FK__ProjectAs__Creat__14270015");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.ProjectAssigns)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__ProjectAs__Lectu__1332DBDC");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectAssigns)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK__ProjectAs__Proje__114A936A");

            entity.HasOne(d => d.Special).WithMany(p => p.ProjectAssigns)
                .HasForeignKey(d => d.SpecialId)
                .HasConstraintName("FK__ProjectAs__Speci__123EB7A3");
        });

        modelBuilder.Entity<ProjectClassMem>(entity =>
        {
            entity.HasKey(e => e.ProClassMemId).HasName("PK__ProjectC__1A19542A32ADDDE2");

            entity.Property(e => e.ProClassMemId)
                .HasMaxLength(155)
                .HasColumnName("ProClassMemID");
            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(155)
                .HasColumnName("LecturerID");
            entity.Property(e => e.ProClassId)
                .HasMaxLength(155)
                .HasColumnName("ProClassID");
            entity.Property(e => e.RoleInClass).HasMaxLength(15);

            entity.HasOne(d => d.Assign).WithMany(p => p.ProjectClassMems)
                .HasForeignKey(d => d.AssignId)
                .HasConstraintName("FK__ProjectCl__Assig__2645B050");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.ProjectClassMems)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__ProjectCl__Lectu__25518C17");

            entity.HasOne(d => d.ProClass).WithMany(p => p.ProjectClassMems)
                .HasForeignKey(d => d.ProClassId)
                .HasConstraintName("FK__ProjectCl__ProCl__245D67DE");
        });

        modelBuilder.Entity<ProjectClassroom>(entity =>
        {
            entity.HasKey(e => e.ProClassId).HasName("PK__ProjectC__2A14BFCCB005D1DD");

            entity.Property(e => e.ProClassId)
                .HasMaxLength(155)
                .HasColumnName("ProClassID");
            entity.Property(e => e.ClassroomName).HasMaxLength(255);
            entity.Property(e => e.InviteCode).HasMaxLength(15);
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.ProjectId)
                .HasMaxLength(155)
                .HasColumnName("ProjectID");

            entity.HasOne(d => d.Project).WithMany(p => p.ProjectClassrooms)
                .HasForeignKey(d => d.ProjectId)
                .HasConstraintName("FK__ProjectCl__Proje__208CD6FA");
        });

        modelBuilder.Entity<ProjectContent>(entity =>
        {
            entity.HasKey(e => e.ProContentId).HasName("PK__ProjectC__7DD7F9EBFC2F2ADF");

            entity.Property(e => e.ProContentId)
                .HasMaxLength(155)
                .HasColumnName("ProContentID");
            entity.Property(e => e.ContentType).HasMaxLength(55);
            entity.Property(e => e.CreateDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.PostedBy).HasMaxLength(155);
            entity.Property(e => e.ProClassId)
                .HasMaxLength(155)
                .HasColumnName("ProClassID");
            entity.Property(e => e.Title).HasMaxLength(255);

            entity.HasOne(d => d.PostedByNavigation).WithMany(p => p.ProjectContents)
                .HasForeignKey(d => d.PostedBy)
                .HasConstraintName("FK__ProjectCo__Poste__3B40CD36");

            entity.HasOne(d => d.ProClass).WithMany(p => p.ProjectContents)
                .HasForeignKey(d => d.ProClassId)
                .HasConstraintName("FK__ProjectCo__ProCl__3A4CA8FD");
        });

        modelBuilder.Entity<ProjectMenber>(entity =>
        {
            entity.HasKey(e => e.MemberId).HasName("PK__ProjectM__0CF04B389CF496D0");

            entity.Property(e => e.MemberId)
                .HasMaxLength(155)
                .HasColumnName("MemberID");
            entity.Property(e => e.RegistId)
                .HasMaxLength(155)
                .HasColumnName("RegistID");
            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");

            entity.HasOne(d => d.Regist).WithMany(p => p.ProjectMenbers)
                .HasForeignKey(d => d.RegistId)
                .HasConstraintName("FK__ProjectMe__Regis__17F790F9");

            entity.HasOne(d => d.Student).WithMany(p => p.ProjectMenbers)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__ProjectMe__Stude__18EBB532");
        });

        modelBuilder.Entity<ProjectSubmission>(entity =>
        {
            entity.HasKey(e => e.ProSubmitId).HasName("PK__ProjectS__DAD5A364D70810E9");

            entity.Property(e => e.ProSubmitId)
                .HasMaxLength(155)
                .HasColumnName("ProSubmitID");
            entity.Property(e => e.ProContentId)
                .HasMaxLength(155)
                .HasColumnName("ProContentID");
            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");
            entity.Property(e => e.SubmitDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.ProContent).WithMany(p => p.ProjectSubmissions)
                .HasForeignKey(d => d.ProContentId)
                .HasConstraintName("FK__ProjectSu__ProCo__41EDCAC5");

            entity.HasOne(d => d.Student).WithMany(p => p.ProjectSubmissions)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__ProjectSu__Stude__42E1EEFE");
        });

        modelBuilder.Entity<ProjectSubmitFile>(entity =>
        {
            entity.HasKey(e => e.ProSubmitFiles).HasName("PK__ProjectS__B0FB83177EED7266");

            entity.Property(e => e.ProSubmitFiles).HasMaxLength(155);
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(555);
            entity.Property(e => e.FileType).HasMaxLength(25);
            entity.Property(e => e.ProSubmitId)
                .HasMaxLength(155)
                .HasColumnName("ProSubmitID");

            entity.HasOne(d => d.ProSubmit).WithMany(p => p.ProjectSubmitFiles)
                .HasForeignKey(d => d.ProSubmitId)
                .HasConstraintName("FK__ProjectSu__ProSu__46B27FE2");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Roles__8AFACE3AE07AC7B6");

            entity.Property(e => e.RoleId)
                .HasMaxLength(155)
                .HasColumnName("RoleID");
            entity.Property(e => e.Description).HasMaxLength(155);
            entity.Property(e => e.RoleName).HasMaxLength(155);
        });

        modelBuilder.Entity<SchoolYear>(entity =>
        {
            entity.HasKey(e => e.YearId).HasName("PK__SchoolYe__C33A18ADEBA09EC5");

            entity.Property(e => e.YearId)
                .HasMaxLength(155)
                .HasColumnName("YearID");
            entity.Property(e => e.DepartId)
                .HasMaxLength(155)
                .HasColumnName("DepartID");
            entity.Property(e => e.YearName).HasMaxLength(50);

            entity.HasOne(d => d.Depart).WithMany(p => p.SchoolYears)
                .HasForeignKey(d => d.DepartId)
                .HasConstraintName("FK__SchoolYea__Depar__412EB0B6");
        });

        modelBuilder.Entity<Semester>(entity =>
        {
            entity.HasKey(e => e.SemesterId).HasName("PK__Semester__043301BDD48958DD");

            entity.Property(e => e.SemesterId)
                .HasMaxLength(155)
                .HasColumnName("SemesterID");
            entity.Property(e => e.DepartId)
                .HasMaxLength(155)
                .HasColumnName("DepartID");
            entity.Property(e => e.SemesterName).HasMaxLength(100);
            entity.Property(e => e.YearId)
                .HasMaxLength(155)
                .HasColumnName("YearID");

            entity.HasOne(d => d.Depart).WithMany(p => p.Semesters)
                .HasForeignKey(d => d.DepartId)
                .HasConstraintName("FK__Semesters__Depar__44FF419A");

            entity.HasOne(d => d.Year).WithMany(p => p.Semesters)
                .HasForeignKey(d => d.YearId)
                .HasConstraintName("FK__Semesters__YearI__440B1D61");
        });

        modelBuilder.Entity<Specialization>(entity =>
        {
            entity.HasKey(e => e.SpecialId).HasName("PK__Speciali__2B5EC240195C35F2");

            entity.Property(e => e.SpecialId)
                .HasMaxLength(155)
                .HasColumnName("SpecialID");
            entity.Property(e => e.MajorId)
                .HasMaxLength(155)
                .HasColumnName("MajorID");
            entity.Property(e => e.SpecialName).HasMaxLength(155);

            entity.HasOne(d => d.Major).WithMany(p => p.Specializations)
                .HasForeignKey(d => d.MajorId)
                .HasConstraintName("FK__Specializ__Major__49C3F6B7");
        });

  
        modelBuilder.Entity<StaffDepart>(entity =>
        {
            entity.HasKey(e => new { e.StaffId, e.DepartId }).HasName("PK__StaffDep__D63194EA91C2DDAA");

            entity.Property(e => e.StaffId)
                .HasMaxLength(155)
                .HasColumnName("StaffID");
            entity.Property(e => e.DepartId)
                .HasMaxLength(155)
                .HasColumnName("DepartID");

            entity.HasOne(e => e.Staff)
                .WithMany(s => s.StaffDeparts)
                .HasForeignKey(e => e.StaffId)
                .OnDelete(DeleteBehavior.ClientSetNull);

            entity.HasOne(e => e.Department)
                .WithMany(d => d.StaffDeparts)
                .HasForeignKey(e => e.DepartId)
                .OnDelete(DeleteBehavior.ClientSetNull);


        });

    modelBuilder.Entity<StuInternWeeklyReport>(entity =>
        {
            entity.HasKey(e => e.StuReprotId).HasName("PK__StuInter__F90A7526D82F4BBD");

            entity.Property(e => e.StuReprotId)
                .HasMaxLength(155)
                .HasColumnName("StuReprotID");
            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");
            entity.Property(e => e.SubmitDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Assign).WithMany(p => p.StuInternWeeklyReports)
                .HasForeignKey(d => d.AssignId)
                .HasConstraintName("FK__StuIntern__Assig__73852659");

            entity.HasOne(d => d.Student).WithMany(p => p.StuInternWeeklyReports)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__StuIntern__Stude__74794A92");
        });

        modelBuilder.Entity<Student>(entity =>
        {
            entity.HasKey(e => e.StudentId).HasName("PK__Students__32C52A79A1BFEA24");

            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");
            entity.Property(e => e.Address).HasMaxLength(255);
            entity.Property(e => e.ClassId)
                .HasMaxLength(155)
                .HasColumnName("ClassID");
            entity.Property(e => e.DepartId)
                .HasMaxLength(155)
                .HasColumnName("DepartID");
            entity.Property(e => e.Dob).HasColumnName("DOB");
            entity.Property(e => e.FirstName).HasMaxLength(20);
            entity.Property(e => e.Image).HasMaxLength(555);
            entity.Property(e => e.LastName).HasMaxLength(50);
            entity.Property(e => e.Mail).HasMaxLength(155);
            entity.Property(e => e.Gender).HasMaxLength(5);
            entity.Property(e => e.PhoneEmer).HasMaxLength(20);
            entity.Property(e => e.PhoneNum).HasMaxLength(20);
            entity.Property(e => e.StuMajor).HasMaxLength(155);
            entity.Property(e => e.UserId)
                .HasMaxLength(155)
                .HasColumnName("UserID");

            entity.HasOne(d => d.Class).WithMany(p => p.Students)
                .HasForeignKey(d => d.ClassId)
                .HasConstraintName("FK__Students__ClassI__571DF1D5");

            entity.HasOne(d => d.Depart).WithMany(p => p.Students)
                .HasForeignKey(d => d.DepartId)
                .HasConstraintName("FK__Students__Depart__5629CD9C");

            entity.HasOne(d => d.StuMajorNavigation).WithMany(p => p.Students)
                .HasForeignKey(d => d.StuMajor)
                .HasConstraintName("FK__Students__StuMaj__5812160E");

            entity.HasOne(d => d.User).WithMany(p => p.Students)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Students__UserID__5535A963");
        });

        modelBuilder.Entity<StudentWeeklyReport>(entity =>
        {
            entity.HasKey(e => e.StuReportId).HasName("PK__StudentW__6F0CC48BE6F63847");

            entity.Property(e => e.StuReportId)
                .HasMaxLength(155)
                .HasColumnName("StuReportID");
            entity.Property(e => e.AssigntId)
                .HasMaxLength(155)
                .HasColumnName("AssigntID");
            entity.Property(e => e.ReportWritter).HasMaxLength(155);
            entity.Property(e => e.SubmitDate).HasDefaultValueSql("(getdate())");

            entity.HasOne(d => d.Assignt).WithMany(p => p.StudentWeeklyReports)
                .HasForeignKey(d => d.AssigntId)
                .HasConstraintName("FK__StudentWe__Assig__3587F3E0");

            entity.HasOne(d => d.ReportWritterNavigation).WithMany(p => p.StudentWeeklyReports)
                .HasForeignKey(d => d.ReportWritter)
                .HasConstraintName("FK__StudentWe__Repor__367C1819");
        });

        modelBuilder.Entity<Subject>(entity =>
        {
            entity.HasKey(e => e.SubId).HasName("PK__Subjects__4D9BB86A311907B5");

            entity.Property(e => e.SubId)
                .HasMaxLength(155)
                .HasColumnName("SubID");
            entity.Property(e => e.DepartId)
                .HasMaxLength(155)
                .HasColumnName("DepartID");
            entity.Property(e => e.Description).HasMaxLength(255);
            entity.Property(e => e.SubCode).HasMaxLength(155);
            entity.Property(e => e.SubMajor).HasMaxLength(155);
            entity.Property(e => e.SubName).HasMaxLength(200);

            entity.HasOne(d => d.Depart).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.DepartId)
                .HasConstraintName("FK__Subjects__Depart__5AEE82B9");

            entity.HasOne(d => d.SubMajorNavigation).WithMany(p => p.Subjects)
                .HasForeignKey(d => d.SubMajor)
                .HasConstraintName("FK__Subjects__SubMaj__5BE2A6F2");
        });

        modelBuilder.Entity<Submission>(entity =>
        {
            entity.HasKey(e => e.SubmitId).HasName("PK__Submissi__421A8E1E29AF4573");

            entity.Property(e => e.SubmitId)
                .HasMaxLength(155)
                .HasColumnName("SubmitID");
            entity.Property(e => e.AssignId)
                .HasMaxLength(155)
                .HasColumnName("AssignID");
            entity.Property(e => e.FeedBack).HasMaxLength(255);
            entity.Property(e => e.SubmitType).HasMaxLength(55);
            entity.Property(e => e.StudentId)
                .HasMaxLength(155)
                .HasColumnName("StudentID");
            entity.Property(e => e.SubmitAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Assign).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.AssignId)
                .HasConstraintName("FK__Submissio__Assig__797309D9");

            entity.HasOne(d => d.Student).WithMany(p => p.Submissions)
                .HasForeignKey(d => d.StudentId)
                .HasConstraintName("FK__Submissio__Stude__7A672E12");
        });

        modelBuilder.Entity<SubmitFile>(entity =>
        {
            entity.HasKey(e => e.FileId).HasName("PK__SubmitFi__6F0F989F9D333D12");

            entity.Property(e => e.FileId)
                .HasMaxLength(155)
                .HasColumnName("FileID");
            entity.Property(e => e.FileName).HasMaxLength(255);
            entity.Property(e => e.FilePath).HasMaxLength(555);
            entity.Property(e => e.FileType).HasMaxLength(50);
            entity.Property(e => e.SubmitId)
                .HasMaxLength(155)
                .HasColumnName("SubmitID");
            entity.Property(e => e.UpdateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");

            entity.HasOne(d => d.Submit).WithMany(p => p.SubmitFiles)
                .HasForeignKey(d => d.SubmitId)
                .HasConstraintName("FK__SubmitFil__Submi__7E37BEF6");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Users__1788CCAC923207EA");

            entity.Property(e => e.UserId)
                .HasMaxLength(155)
                .HasColumnName("UserID");
            entity.Property(e => e.CreateAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.PasswordHash).HasMaxLength(555);
            entity.Property(e => e.RoleId)
                .HasMaxLength(155)
                .HasColumnName("RoleID");
            entity.Property(e => e.Status).HasMaxLength(155);
            entity.Property(e => e.Username).HasMaxLength(55);

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Users__RoleID__398D8EEE");
        });

        modelBuilder.Entity<WeeklyMeetAttendence>(entity =>
        {
            entity.HasKey(e => e.MeetAttendenceId).HasName("PK__WeeklyMe__2BFE47CC008976A8");

            entity.Property(e => e.MeetAttendenceId)
                .HasMaxLength(155)
                .HasColumnName("MeetAttendenceID");
            entity.Property(e => e.MeetingId)
                .HasMaxLength(155)
                .HasColumnName("MeetingID");
            entity.Property(e => e.Status).HasMaxLength(25);

            entity.HasOne(d => d.Meeting).WithMany(p => p.WeeklyMeetAttendences)
                .HasForeignKey(d => d.MeetingId)
                .HasConstraintName("FK__WeeklyMee__Meeti__2CF2ADDF");
        });

        modelBuilder.Entity<WeeklyMeeting>(entity =>
        {
            entity.HasKey(e => e.MeetingId).HasName("PK__WeeklyMe__E9F9E9AC40A17528");

            entity.Property(e => e.MeetingId)
                .HasMaxLength(155)
                .HasColumnName("MeetingID");
            entity.Property(e => e.LecturerId)
                .HasMaxLength(155)
                .HasColumnName("LecturerID");
            entity.Property(e => e.ProClassId)
                .HasMaxLength(155)
                .HasColumnName("ProClassID");

            entity.HasOne(d => d.Lecturer).WithMany(p => p.WeeklyMeetings)
                .HasForeignKey(d => d.LecturerId)
                .HasConstraintName("FK__WeeklyMee__Lectu__2A164134");

            entity.HasOne(d => d.ProClass).WithMany(p => p.WeeklyMeetings)
                .HasForeignKey(d => d.ProClassId)
                .HasConstraintName("FK__WeeklyMee__ProCl__29221CFB");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
