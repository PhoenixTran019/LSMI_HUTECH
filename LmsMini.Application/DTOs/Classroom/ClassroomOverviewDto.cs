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

        public string? ClassName { get; set;}

        public string? ClassSub {  get; set; }

        public string? LecturerName { get; set; }

        public string? Description { get; set; }

        public string? Status { get; set; }

        public string? InviteCode { get; set; }

        public List<LessonViewDto> Lessons { get; set; } = new List<LessonViewDto>();

        public List<AssignmentViewDto> Assignments { get; set; } = new List<AssignmentViewDto>();
    }
}
