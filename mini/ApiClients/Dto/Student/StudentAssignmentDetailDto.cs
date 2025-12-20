using System;
using System.Collections.Generic;

namespace LmsMini.WebApp.ApiClients.Dto.Student
{
    public class StudentAssignmentDetailDto
    {
        public string? AssignId { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? DeadlineStatus { get; set; }
        public string? HomeworkStatus { get; set; }
        public DateTime? CreateAt { get; set; }

        // Danh sách file đính kèm bài tập từ giáo viên
        public List<StudentAssignmentFileDto> Files { get; set; } = new();

        // Bài làm mới nhất của sinh viên (nếu có)
        public StudentLatestSubmissionDto? LatestSubmission { get; set; }
    }

    public class StudentAssignmentFileDto
    {
        // Backend Assignment trả về "FileId"
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

    public class StudentSubmitFileDto
    {
        public string? FileId { get; set; }
        public string? FileName { get; set; }
        public string? FileType { get; set; }
    }
}