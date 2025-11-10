using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class Classroom
{
    public string ClassroomId { get; set; } = null!;

    public string? ClassName { get; set; }

    public string? ClassSub { get; set; }

    public string? Description { get; set; }

    public string? InviteCode { get; set; }

    public string? CreateBy { get; set; }

    public string? ClassStatus { get; set; }

    public virtual ICollection<Assignment> Assignments { get; set; } = new List<Assignment>();

    public virtual Subject? ClassSubNavigation { get; set; }

    public virtual ICollection<ClassroomMember> ClassroomMembers { get; set; } = new List<ClassroomMember>();

    public virtual DepartmentStaff? CreateByNavigation { get; set; }

    public virtual ICollection<Lesson> Lessons { get; set; } = new List<Lesson>();
}
