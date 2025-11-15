using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class ProjectClassMem
{
    public string ProClassMemId { get; set; } = null!;

    public string? ProClassId { get; set; }

    public string? LecturerId { get; set; }

    public string? AssignId { get; set; }

    public string? RoleInClass { get; set; }

    public virtual ProjectAssign? Assign { get; set; }

    public virtual DepartmentStaff? Lecturer { get; set; }

    public virtual ProjectClassroom? ProClass { get; set; }
}
