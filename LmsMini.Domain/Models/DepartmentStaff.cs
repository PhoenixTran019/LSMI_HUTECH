using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class DepartmentStaff
{
    public string StaffId { get; set; } = null!;

    public string? UserId { get; set; }

    public string? Majors { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public DateOnly? Dob { get; set; }

    public string? PhoneNum { get; set; }

    public string? Description { get; set; }

    public string? Mail { get; set; }

    public string? Address { get; set; }

    public string? Image { get; set; }

    public string? Status { get; set; }

    public DateTime? HireDate { get; set; }

    public string? StaffRole { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual ICollection<ClassroomMember> ClassroomMembers { get; set; } = new List<ClassroomMember>();

    public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();

    public virtual ICollection<InternAppoval> InternAppovals { get; set; } = new List<InternAppoval>();

    public virtual ICollection<InternAssignment> InternAssignments { get; set; } = new List<InternAssignment>();

    public virtual ICollection<InternClassroom> InternClassrooms { get; set; } = new List<InternClassroom>();

    public virtual ICollection<InternContent> InternContents { get; set; } = new List<InternContent>();

    public virtual ICollection<Internship> Internships { get; set; } = new List<Internship>();

    public virtual ICollection<LecturerInternReport> LecturerInternReports { get; set; } = new List<LecturerInternReport>();

    public virtual ICollection<LecturerWeeklyReport> LecturerWeeklyReports { get; set; } = new List<LecturerWeeklyReport>();

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual Major? MajorsNavigation { get; set; }

    public virtual ICollection<ProjectApproval> ProjectApprovals { get; set; } = new List<ProjectApproval>();

    public virtual ICollection<ProjectAssign> ProjectAssigns { get; set; } = new List<ProjectAssign>();

    public virtual ICollection<ProjectClassMem> ProjectClassMems { get; set; } = new List<ProjectClassMem>();

    public virtual ICollection<ProjectContent> ProjectContents { get; set; } = new List<ProjectContent>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual Role? StaffRoleNavigation { get; set; }

    public virtual User? User { get; set; }

    public virtual ICollection<WeeklyMeeting> WeeklyMeetings { get; set; } = new List<WeeklyMeeting>();

    public virtual ICollection<StaffDepart> StaffDeparts { get; set; } = new List<StaffDepart>();
}
