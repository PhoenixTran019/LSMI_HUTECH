using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class Semester
{
    public string SemesterId { get; set; } = null!;

    public string? YearId { get; set; }

    public string? DepartId { get; set; }

    public string? SemesterName { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? EndDate { get; set; }

    public virtual Department? Depart { get; set; }

    public virtual SchoolYear? Year { get; set; }
}
