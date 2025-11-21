using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Lesson
{
    public class LessonDetailDto
    {
        public string? LessonId { get; set; }
        public string? Title { get; set; }
        public string? Content { get; set; }
        public DateTime? CreatedAt { get; set; }
        public List<LessonFileDto> Files { get; set; }
    }
}
