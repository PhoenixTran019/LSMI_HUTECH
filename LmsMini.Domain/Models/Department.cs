using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class Department
{
    public string DepartId { get; set; } = null!;

    public string? DepartName { get; set; }

    public string? DepartDescription { get; set; }

    public string? Address { get; set; }

    public string? Phone { get; set; }

    public string? Image { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? ApprovalDate { get; set; }

    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();
    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();

    public virtual ICollection<SchoolYear> SchoolYears { get; set; } = new List<SchoolYear>();

    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    public virtual ICollection<StaffDepart> StaffDeparts { get; set; } = new List<StaffDepart>();
}
