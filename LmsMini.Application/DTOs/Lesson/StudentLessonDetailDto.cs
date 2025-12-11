using System;
using System.Collections.Generic;

namespace LmsMini.Application.DTOs.Lesson
{
    public class StudentLessonDetailDto
    {
        public string LessonId { get; set; } = null!;
        public string ClassroomId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string? Content { get; set; }
        public DateTime? CreateAt { get; set; }

        public List<StudentLessonFileDto> Files { get; set; } = new();
    }
}
