using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class WeeklyMeeting
{
    public string MeetingId { get; set; } = null!;

    public string? ProClassId { get; set; }

    public string? LecturerId { get; set; }

    public DateTime? MeetingDate { get; set; }

    public string? Notes { get; set; }

    public virtual DepartmentStaff? Lecturer { get; set; }

    public virtual ICollection<LecturerWeeklyReport> LecturerWeeklyReports { get; set; } = new List<LecturerWeeklyReport>();

    public virtual ProjectClassroom? ProClass { get; set; }

    public virtual ICollection<WeeklyMeetAttendence> WeeklyMeetAttendences { get; set; } = new List<WeeklyMeetAttendence>();
}
