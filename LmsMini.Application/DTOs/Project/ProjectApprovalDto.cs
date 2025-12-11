using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Project
{
    public class ProjectApprovalDto
    {
        public string AssignID { get; set; }

        public string? LecturerID { get; set; }

        public string? Decicion { get; set; }

        public string? Comments { get; set; }
    }
}
