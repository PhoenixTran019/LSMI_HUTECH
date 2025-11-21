using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class Assignment
{
    public string AssignId { get; set; } = null!;

    public string? ClassroomId { get; set; }

    public string? TeacherId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public DateTime? Deadline { get; set; }

    public string? DeadlineStatus { get; set; }

    public string? HomeworkStatus { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<AssignmentFile> AssignmentFiles { get; set; } = new List<AssignmentFile>();

    public virtual Classroom? Classroom { get; set; }

    public virtual ICollection<Submission> Submissions { get; set; } = new List<Submission>();

    public virtual DepartmentStaff? Teacher { get; set; }
}
