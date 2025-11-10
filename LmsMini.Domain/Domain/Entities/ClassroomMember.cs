using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class ClassroomMember
{
    public string MemberId { get; set; } = null!;

    public string? ClassroomId { get; set; }

    public string? LecturerId { get; set; }

    public string? StudentId { get; set; }

    public string? RoleInClass { get; set; }

    public virtual Classroom? Classroom { get; set; }

    public virtual DepartmentStaff? Lecturer { get; set; }

    public virtual Student? Student { get; set; }
}
