using Azure.Core.Pipeline;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class ProjectClassroomDashboardDto
    {
        public string ProClassID { get; set; }

        public string? ClassroomName { get; set; }

        public string? ProjectID { get; set; }

        public string? InviteCode { get; set; }

        public DateTime? CreateDate { get; set; }

        public List<ProjectContentItemDto>? Contents { get; set; }
    }
}
