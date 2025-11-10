using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class StuInternWeeklyReport
{
    public string StuReprotId { get; set; } = null!;

    public string? AssignId { get; set; }

    public string? StudentId { get; set; }

    public int? WeekNumber { get; set; }

    public string? ReportContent { get; set; }

    public DateTime? SubmitDate { get; set; }

    public virtual InternAssignment? Assign { get; set; }

    public virtual Student? Student { get; set; }
}
