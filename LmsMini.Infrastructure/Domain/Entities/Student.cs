using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class Student
{
    public string StudentId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? DepartId { get; set; }

    public string? ClassId { get; set; }

    public string? StuMajor { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public string? PhoneNum { get; set; }

    public string? PhoneEmer { get; set; }

    public string? Mail { get; set; }

    public DateOnly? Dob { get; set; }

    public string? Address { get; set; }

    public string? Image { get; set; }

    public DateOnly? EnrollmentDate { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Class? Class { get; set; }

    public virtual ICollection<ClassroomMember> ClassroomMembers { get; set; } = new List<ClassroomMember>();

    public virtual Department? Depart { get; set; }

    public virtual ICollection<InternRegist> InternRegists { get; set; } = new List<InternRegist>();

    public virtual ICollection<InternSubmission> InternSubmissions { get; set; } = new List<InternSubmission>();

    public virtual ICollection<ProjectAssign> ProjectAssigns { get; set; } = new List<ProjectAssign>();

    public virtual ICollection<ProjectMenber> ProjectMenbers { get; set; } = new List<ProjectMenber>();

    public virtual ICollection<ProjectSubmission> ProjectSubmissions { get; set; } = new List<ProjectSubmission>();

    public virtual ICollection<StuInternWeeklyReport> StuInternWeeklyReports { get; set; } = new List<StuInternWeeklyReport>();

    public virtual Major? StuMajorNavigation { get; set; }

    public virtual ICollection<StudentWeeklyReport> StudentWeeklyReports { get; set; } = new List<StudentWeeklyReport>();

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual User? User { get; set; }
}
