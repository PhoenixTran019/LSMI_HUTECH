using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Classroom
{
    public class ClassroomMemberListItemDto
    {
        public string MemberId { get; set; } = null!;
        public string? RoleInClass { get; set; }
        public string? StudentId { get; set; }
        public string? LecturerId { get; set; }
        public string? FullName { get; set; }
        public string? Mail { get; set; }
    }
}

