using System;
using System.Collections.Generic;

namespace LmsMini.Application.DTOs.Assignment
{
    public class StudentAssignmentDetailDto
    {
        public string AssignmentId { get; set; } = null!;
        public string ClassroomId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Description { get; set; }

        public DateTime? Deadline { get; set; }
        public string? DeadlineStatus { get; set; }
        public string? HomeworkStatus { get; set; }

        public bool IsSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public double? Grade { get; set; }
        public string? Feedback { get; set; }

        public List<StudentAssignmentFileDto> Files { get; set; } = new();
    }
}
