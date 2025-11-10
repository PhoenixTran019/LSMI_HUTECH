using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class ProConFile
{
    public string ProConFileId { get; set; } = null!;

    public string? ProContentId { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? FileType { get; set; }

    public virtual ProjectContent? ProContent { get; set; }
}
