using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class ProjectContentItemDto
    {
        public string ProContentID { get; set; }

        public string? Title { get; set; }

        public string? ContentType { get; set; }

        public DateTime? CreatedDate { get; set; }

        public DateTime? Deadline { get; set;}

        public string? PostedByName { get; set; }
    }
}
