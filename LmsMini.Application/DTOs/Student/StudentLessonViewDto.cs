using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Student
{
    public class StudentLessonViewDto
    {
        public string? LessonId { get; set; }

        public string? Title { get; set; }

        public string? Content { get; set; }

        public DateTime? CreateAt { get; set; }

        public List<FileDto>? Files { get; set; } = new();
    }
}
