using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class InternContent
{
    public string InternContentId { get; set; } = null!;

    public string? InternClassId { get; set; }

    public string? PostedBy { get; set; }

    public string? Title { get; set; }

    public string? ContentText { get; set; }

    public DateTime? Deadline { get; set; }

    public string? ContentType { get; set; }

    public virtual ICollection<InConFile> InConFiles { get; set; } = new List<InConFile>();

    public virtual InternClassroom? InternClass { get; set; }

    public virtual ICollection<InternSubmission> InternSubmissions { get; set; } = new List<InternSubmission>();

    public virtual DepartmentStaff? PostedByNavigation { get; set; }
}
