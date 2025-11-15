using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class ProjectAssign
{
    public string AssignId { get; set; } = null!;

    public string? ProjectId { get; set; }

    public string? SpecialId { get; set; }

    public string? GroupName { get; set; }

    public string? Description { get; set; }

    public string? LecturerId { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? Status { get; set; }

    public string? ApprovalNote { get; set; }

    public virtual Student? CreateByNavigation { get; set; }

    public virtual DepartmentStaff? Lecturer { get; set; }

    public virtual ICollection<LecturerWeeklyReport> LecturerWeeklyReports { get; set; } = new List<LecturerWeeklyReport>();

    public virtual Project? Project { get; set; }

    public virtual ICollection<ProjectApproval> ProjectApprovals { get; set; } = new List<ProjectApproval>();

    public virtual ICollection<ProjectClassMem> ProjectClassMems { get; set; } = new List<ProjectClassMem>();

    public virtual ICollection<ProjectMenber> ProjectMenbers { get; set; } = new List<ProjectMenber>();

    public virtual Specialization? Special { get; set; }

    public virtual ICollection<StudentWeeklyReport> StudentWeeklyReports { get; set; } = new List<StudentWeeklyReport>();
}
