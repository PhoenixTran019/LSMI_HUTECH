using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class Internship
{
    public string InternId { get; set; } = null!;

    public string? Title { get; set; }

    public string? MajorId { get; set; }

    public string? Cohort { get; set; }

    public string? AvailForm { get; set; }

    public string? Description { get; set; }

    public int? MaxStudent { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public string? CreateBy { get; set; }

    public DateTime? CreateDate { get; set; }

    public virtual DepartmentStaff? CreateByNavigation { get; set; }

    public virtual ICollection<InternClassroom> InternClassrooms { get; set; } = new List<InternClassroom>();

    public virtual ICollection<InternRegist> InternRegists { get; set; } = new List<InternRegist>();

    public virtual Major? Major { get; set; }
}
