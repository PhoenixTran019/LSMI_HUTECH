using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class InternClassroom
{
    public string InternClassId { get; set; } = null!;

    public string? LecturerId { get; set; }

    public string? InternId { get; set; }

    public string? ClassroomName { get; set; }

    public string? Description { get; set; }

    public string? InviteCode { get; set; }

    public DateTime? CreateDate { get; set; }

    public virtual Internship? Intern { get; set; }

    public virtual ICollection<InternClassMem> InternClassMems { get; set; } = new List<InternClassMem>();

    public virtual ICollection<InternContent> InternContents { get; set; } = new List<InternContent>();

    public virtual DepartmentStaff? Lecturer { get; set; }
}
