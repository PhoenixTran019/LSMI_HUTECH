// ApiClients/Dto/Student/AssignmentDetailDto.cs

using System.Collections.Generic;
using System;

namespace LmsMini.WebApp.ApiClients.Dto.Student // <--- THÊM NAMESPACE NÀY
{
    public class AssignmentDetailDto
    {
        public string AssignId { get; set; } = "";
        public string Title { get; set; } = "";
        public string Description { get; set; } = "";
        public DateTime Deadline { get; set; }
        public string ClassroomName { get; set; } = "";
        public List<AssignmentFileDto> AttachedFiles { get; set; } = new List<AssignmentFileDto>();
        public SubmissionDetailDto? StudentSubmission { get; set; }
    }

    public class AssignmentFileDto
    {
        public string FileId { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
    }

    public class SubmissionDetailDto
    {
        public string SubmitId { get; set; } = "";
        public DateTime SubmitAt { get; set; }
        public double? Grade { get; set; }
        public string? Feedback { get; set; }
        public List<SubmissionFileDto> SubmittedFiles { get; set; } = new List<SubmissionFileDto>();

        public string Status { get; set; } = "Đã nộp";
    }

    public class SubmissionFileDto
    {
        public string FileId { get; set; } = "";
        public string FileName { get; set; } = "";
        public string FilePath { get; set; } = "";
    }
}