using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Classroom
{
    public class LessonViewDto
    {
        public string? LessonId { get; set; }
        public string? Title { get; set; }
        public DateTime? CreateAt { get; set; }
    }
}
