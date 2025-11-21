using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class Class
{
    public string ClassId { get; set; } = null!;

    public string? ClassName { get; set; }

    public string? ClassMajor { get; set; }

    public string? Course { get; set; }

    public string? DepartId { get; set; }

    public virtual Major? ClassMajorNavigation { get; set; }

    public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();

    public virtual Department? Depart { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
