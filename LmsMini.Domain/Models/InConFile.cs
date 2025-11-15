using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class InConFile
{
    public string InConFileId { get; set; } = null!;

    public string? InternContentId { get; set; }

    public string? FileName { get; set; }

    public string? FilePaht { get; set; }

    public string? FileType { get; set; }

    public virtual InternContent? InternContent { get; set; }
}
