using System;

namespace LmsMini.Application.DTOs
{
    /// <summary>
    /// Item hiển thị trong danh sách Lesson của một Classroom.
    /// </summary>
    public class StudentLessonListItemDto
    {
        public string LessonId { get; set; } = null!;
        public string ClassroomId { get; set; } = null!;
        public string Title { get; set; } = null!;
        public DateTime? CreateAt { get; set; }
        public int FileCount { get; set; }
    }
}
