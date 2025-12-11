using System;
using System.Collections.Generic;

namespace LmsMini.Application.DTOs.Assignment
{
    public class StudentSubmissionDto
    {
        public string SubmitId { get; set; } = null!;
        public string AssignmentId { get; set; } = null!;
        public DateTime? SubmitAt { get; set; }
        public string? Feedback { get; set; }
        public double? Grade { get; set; }

        public List<StudentSubmissionFileDto> Files { get; set; } = new();
    }
}
