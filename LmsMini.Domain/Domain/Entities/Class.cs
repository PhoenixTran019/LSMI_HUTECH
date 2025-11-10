using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class Class
{
    public string ClassId { get; set; } = null!;

    public string? ClassName { get; set; }

    public string? ClassMajor { get; set; }

    public virtual Major? ClassMajorNavigation { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
