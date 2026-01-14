namespace LmsMini.Application.DTOs.Student
{
    public class StudentAssignmentListItemDto
    {
        public string? AssignId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? DeadlineStatus { get; set; }
        public string? HomeworkStatus { get; set; }
        public DateTime? CreateAt { get; set; }
        public int FileCount { get; set; }

        public bool HasSubmitted { get; set; }
        public DateTime? SubmittedAt { get; set; }
        public bool IsLate { get; set; }
        public string? SubmitType { get; set; }
    }

    public class StudentAssignmentFileDto
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
    }

    public class StudentSubmitFileDto
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
    }

    public class StudentLatestSubmissionDto
    {
        public string? SubmitId { get; set; }
        public DateTime? SubmitAt { get; set; }
        public string? SubmitType { get; set; }
        public string? FeedBack { get; set; }
        public decimal? Grade { get; set; }
        public List<StudentSubmitFileDto> Files { get; set; } = new();
    }

    public class StudentAssignmentDetailDto
    {
        public string? AssignId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? DeadlineStatus { get; set; }
        public string? HomeworkStatus { get; set; }
        public DateTime? CreateAt { get; set; }

        public List<StudentAssignmentFileDto> Files { get; set; } = new();
        public StudentLatestSubmissionDto? LatestSubmission { get; set; }
    }

    public class StudentSubmitResultDto
    {
        public string SubmitId { get; set; } = default!;
        public string AssignId { get; set; } = default!;
        public string StudentId { get; set; } = default!;
        public DateTime SubmitAt { get; set; }
        public string SubmitType { get; set; } = default!;
    }
}
