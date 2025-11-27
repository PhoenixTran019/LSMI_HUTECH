using System;

namespace LmsMini.Application.DTOs
{
    public class StudentLessonFileDto
    {
        public string LessonFileId { get; set; } = null!;
        public string FileName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string? FileType { get; set; }
        public DateTime? UploadAt { get; set; }
    }
}
