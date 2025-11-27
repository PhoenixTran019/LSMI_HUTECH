using System;

namespace LmsMini.Application.DTOs
{
    /// <summary>
    /// Item trong danh sách Assignment của một Classroom.
    /// </summary>
    public class StudentAssignmentListItemDto
    {
        public string AssignmentId { get; set; } = null!;
        public string ClassroomId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime? Deadline { get; set; }
        public string? DeadlineStatus { get; set; }
        public string? HomeworkStatus { get; set; }

        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public double? Grade { get; set; }
    }
}
