using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class StudentWeeklyReport
{
    public string StuReportId { get; set; } = null!;

    public string? AssigntId { get; set; }

    public string? ReportWritter { get; set; }

    public int? WeekNumber { get; set; }

    public string? ReportContent { get; set; }

    public DateTime? SubmitDate { get; set; }

    public virtual ProjectAssign? Assignt { get; set; }

    public virtual Student? ReportWritterNavigation { get; set; }
}
