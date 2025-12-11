using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class ProjectSubmission
{
    public string ProSubmitId { get; set; } = null!;

    public string? ProContentId { get; set; }

    public string? StudentId { get; set; }

    public DateTime? SubmitDate { get; set; }

    public double? Score { get; set; }

    public string? FeedBack { get; set; }

    public virtual ProjectContent? ProContent { get; set; }

    public virtual ICollection<ProjectSubmitFile> ProjectSubmitFiles { get; set; } = new List<ProjectSubmitFile>();

    public virtual Student? Student { get; set; }
}
