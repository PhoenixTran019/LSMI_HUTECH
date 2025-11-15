using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class Project
{
    public string ProjectId { get; set; } = null!;

    public string? Title { get; set; }

    public string? ProMajor { get; set; }

    public string? Cohort { get; set; }

    public int? MaxStudents { get; set; }

    public string? Description { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public virtual DepartmentStaff? CreateByNavigation { get; set; }

    public virtual Major? ProMajorNavigation { get; set; }

    public virtual ICollection<ProjectAssign> ProjectAssigns { get; set; } = new List<ProjectAssign>();

    public virtual ICollection<ProjectClassroom> ProjectClassrooms { get; set; } = new List<ProjectClassroom>();
}
