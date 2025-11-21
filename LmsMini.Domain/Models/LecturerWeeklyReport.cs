using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class LecturerWeeklyReport
{
    public string LecReportId { get; set; } = null!;

    public string? AssigntId { get; set; }

    public string? ReportWritter { get; set; }

    public string? WeeklyMeeting { get; set; }

    public int? WeekNumber { get; set; }

    public string? ReportContent { get; set; }

    public DateTime? SubmitDate { get; set; }

    public virtual ProjectAssign? Assignt { get; set; }

    public virtual DepartmentStaff? ReportWritterNavigation { get; set; }

    public virtual WeeklyMeeting? WeeklyMeetingNavigation { get; set; }
}
