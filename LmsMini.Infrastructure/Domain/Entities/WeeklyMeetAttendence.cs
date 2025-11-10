using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class WeeklyMeetAttendence
{
    public string MeetAttendenceId { get; set; } = null!;

    public string? Status { get; set; }

    public string? MeetingId { get; set; }

    public virtual WeeklyMeeting? Meeting { get; set; }
}
