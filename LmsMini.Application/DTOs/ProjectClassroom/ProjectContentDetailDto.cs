using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class ProjectContentDetailDto
    {
        public string ProContentID { get; set; }

        public string? Title { get; set; }

        public string? ContentText { get; set; }

        public string? PostedBy { get; set; }

        public DateTime? CreateDate { get; set; }

        public string? ContentType { get; set; }

        public DateTime? Deadline { get; set; }

        public List<FileDto>? Files { get; set; } = new();

        public List<ProjectStudentSubmitDto> ProStudentSubmitted { get; set; }

        public List<PorjectStudentInfoDto> ProStudentNotSubmitted { get; set; }
    }
}
