using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class InternClassMem
{
    public string MemberId { get; set; } = null!;

    public string? InternClassId { get; set; }

    public string? AssignId { get; set; }

    public virtual InternAssignment? Assign { get; set; }

    public virtual InternClassroom? InternClass { get; set; }
}
