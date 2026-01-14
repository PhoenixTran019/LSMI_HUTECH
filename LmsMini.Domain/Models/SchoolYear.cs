using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class SchoolYear
{
    public string YearId { get; set; } = null!;

    public string? DepartId { get; set; }

    public string? YearName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Department? Depart { get; set; }

    public virtual ICollection<Semester> Semesters { get; set; } = new List<Semester>();
}
