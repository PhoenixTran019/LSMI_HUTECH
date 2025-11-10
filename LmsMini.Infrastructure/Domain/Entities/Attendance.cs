using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class Attendance
{
    public string AttendanceId { get; set; } = null!;

    public string? LessonId { get; set; }

    public string? StudentId { get; set; }

    public string? LecturerId { get; set; }

    public string? Status { get; set; }

    public DateTime? CheckInTime { get; set; }

    public virtual DepartmentStaff? Lecturer { get; set; }

    public virtual Lesson? Lesson { get; set; }

    public virtual Student? Student { get; set; }
}
