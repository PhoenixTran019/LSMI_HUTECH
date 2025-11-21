using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class Lesson
{
    public string LessonId { get; set; } = null!;

    public string? CreateBy { get; set; }

    public string? ClassroomId { get; set; }

    public string? Title { get; set; }

    public string? Content { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual Classroom? Classroom { get; set; }

    public virtual DepartmentStaff? CreateByNavigation { get; set; }

    public virtual ICollection<LessonFile> LessonFiles { get; set; } = new List<LessonFile>();
}
