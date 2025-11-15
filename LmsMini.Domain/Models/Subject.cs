using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class Subject
{
    public string SubId { get; set; } = null!;

    public string? DepartId { get; set; }

    public string? SubMajor { get; set; }

    public string? SubName { get; set; }

    public string? SubCode { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Classroom> Classrooms { get; set; } = new List<Classroom>();

    public virtual Department? Depart { get; set; }

    public virtual Major? SubMajorNavigation { get; set; }
}
