using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class SubmitFile
{
    public string FileId { get; set; } = null!;

    public string? SubmitId { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? FileType { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual Submission? Submit { get; set; }
}
