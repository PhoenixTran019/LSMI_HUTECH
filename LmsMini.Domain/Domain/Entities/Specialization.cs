using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class Specialization
{
    public string SpecialId { get; set; } = null!;

    public string? MajorId { get; set; }

    public string? SpecialName { get; set; }

    public virtual Major? Major { get; set; }

    public virtual ICollection<ProjectAssign> ProjectAssigns { get; set; } = new List<ProjectAssign>();
}
