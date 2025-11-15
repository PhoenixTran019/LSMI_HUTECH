using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Classroom
{
    public class ClassroomOverviewDto
    {
        public string ClassroomId { get; set; }
        public List<LessonViewDto> Lessons { get; set; }
        public List<AssignmentViewDto> Assignments { get; set; }
    }
}
