using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class ProjectContent
{
    public string ProContentId { get; set; } = null!;

    public string? ProClassId { get; set; }

    public string? PostedBy { get; set; }

    public string? Title { get; set; }

    public string? ContentText { get; set; }

    public DateTime? Deadline { get; set; }

    public DateTime? CreateDate { get; set; }

    public string? ContentType { get; set; }

    public virtual DepartmentStaff? PostedByNavigation { get; set; }

    public virtual ProjectClassroom? ProClass { get; set; }

    public virtual ICollection<ProConFile> ProConFiles { get; set; } = new List<ProConFile>();

    public virtual ICollection<ProjectSubmission> ProjectSubmissions { get; set; } = new List<ProjectSubmission>();
}
