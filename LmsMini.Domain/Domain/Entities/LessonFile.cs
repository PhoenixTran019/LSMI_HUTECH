using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class LessonFile
{
    public string FilesId { get; set; } = null!;

    public string? LessonId { get; set; }

    public string? FileName { get; set; }

    public string? FilePath { get; set; }

    public string? FileType { get; set; }

    public DateTime? UpdateAt { get; set; }

    public virtual Lesson? Lesson { get; set; }
}
