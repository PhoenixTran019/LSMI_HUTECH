using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class InternAssignment
{
    public string AssignId { get; set; } = null!;

    public string? RegistId { get; set; }

    public string? LecturerId { get; set; }

    public virtual ICollection<InternClassMem> InternClassMems { get; set; } = new List<InternClassMem>();

    public virtual DepartmentStaff? Lecturer { get; set; }

    public virtual ICollection<LecturerInternReport> LecturerInternReports { get; set; } = new List<LecturerInternReport>();

    public virtual InternRegist? Regist { get; set; }

    public virtual ICollection<StuInternWeeklyReport> StuInternWeeklyReports { get; set; } = new List<StuInternWeeklyReport>();
}
