using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class ProjectClassroomDto
    {
        public string ProClassID {  get; set; }

        public string ProjectID { get; set; }

        public string ClassroomName { get; set; }

        public string InviteCode { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? IsActive { get; set; }

        public List<ClassMemberDto> Members { get; set; } = new();

    }
}
