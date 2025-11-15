using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Classroom
{
    public class AssignmentViewDto
    {
        public string? AssignId { get; set; }
        public string? Title { get; set; }
        public DateTime? Deadline { get; set; }
        public string? DeadlineStatus { get; set; }
    }
}
