using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class InternSubmission
{
    public string SubmitId { get; set; } = null!;

    public string? InternContentId { get; set; }

    public string? StudentId { get; set; }

    public DateTime? SubmitDate { get; set; }

    public string? Feedback { get; set; }

    public virtual InternContent? InternContent { get; set; }

    public virtual Student? Student { get; set; }
}
