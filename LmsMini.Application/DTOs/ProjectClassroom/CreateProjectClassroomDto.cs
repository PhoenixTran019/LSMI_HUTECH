using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class CreateProjectClassroomDto
    {
        public string ProjectID { get; set; }

        public string? ClassroomName { get; set; }

        public string? CreatorLecturerID { get; set; }
    }
}
