using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class ProjectClassroom
{
    public string ProClassId { get; set; } = null!;

    public string? ProjectId { get; set; }

    public string? ClassroomName { get; set; }

    public DateTime? CreateDate { get; set; }

    public bool? IsActive { get; set; }

    public virtual Project? Project { get; set; }

    public virtual ICollection<ProjectClassMem> ProjectClassMems { get; set; } = new List<ProjectClassMem>();

    public virtual ICollection<ProjectContent> ProjectContents { get; set; } = new List<ProjectContent>();

    public virtual ICollection<WeeklyMeeting> WeeklyMeetings { get; set; } = new List<WeeklyMeeting>();
}
