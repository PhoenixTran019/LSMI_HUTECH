using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class AssignmentFile
{
    public string FileId { get; set; } = null!;

    public string? AssignId { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? FileType { get; set; }

    public virtual Assignment? Assign { get; set; }
}
