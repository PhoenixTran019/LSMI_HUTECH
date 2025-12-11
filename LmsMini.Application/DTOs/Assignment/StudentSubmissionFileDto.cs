using System;

namespace LmsMini.Application.DTOs.Assignment
{
    public class StudentSubmissionFileDto
    {
        public string SubmitFileId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? FileType { get; set; }
        public DateTime? UploadAt { get; set; }
    }
}
