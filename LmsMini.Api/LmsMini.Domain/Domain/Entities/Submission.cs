using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class Submission
{
    public string SubmitId { get; set; } = null!;

    public string? AssignId { get; set; }

    public string? StudentId { get; set; }

    public DateTime? SubmitAt { get; set; }

    public string? FeedBack { get; set; }

    public double? Grade { get; set; }

    public virtual Assignment? Assign { get; set; }

    public virtual Student? Student { get; set; }

    public virtual ICollection<SubmitFile> SubmitFiles { get; set; } = new List<SubmitFile>();
}
