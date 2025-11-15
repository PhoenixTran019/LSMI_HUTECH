using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class ProjectMenber
{
    public string MemberId { get; set; } = null!;

    public string? RegistId { get; set; }

    public string? StudentId { get; set; }

    public virtual ProjectAssign? Regist { get; set; }

    public virtual Student? Student { get; set; }
}
