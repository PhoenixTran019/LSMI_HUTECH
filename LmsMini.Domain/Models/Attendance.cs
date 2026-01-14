using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class Attendance
{
    public string AttendanceId { get; set; } = null!;

    public string? StudentId { get; set; }

    public string? ClassroomId { get; set; }

    public string? LecturerId { get; set; }

    public string? Status { get; set; }

    public DateTime? CheckInTime { get; set; }

    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }

    public virtual DepartmentStaff? Lecturer { get; set; }

    public virtual Classroom? Classroom { get; set; }

    public virtual Student? Student { get; set; }
}
