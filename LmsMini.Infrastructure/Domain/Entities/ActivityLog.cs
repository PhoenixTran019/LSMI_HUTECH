using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class ActivityLog
{
    public string LogId { get; set; } = null!;

    public string? StaffId { get; set; }

    public string? DepartId { get; set; }

    public string? Action { get; set; }

    public string? TargetTable { get; set; }

    public string? TargetId { get; set; }

    public string? TargetName { get; set; }

    public DateTime? Timestap { get; set; }

    public virtual Department? Depart { get; set; }

    public virtual DepartmentStaff? Staff { get; set; }
}
