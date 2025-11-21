using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class ProjectSubmitFile
{
    public string ProSubmitFiles { get; set; } = null!;

    public string? ProSubmitId { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? FileType { get; set; }

    public virtual ProjectSubmission? ProSubmit { get; set; }
}
