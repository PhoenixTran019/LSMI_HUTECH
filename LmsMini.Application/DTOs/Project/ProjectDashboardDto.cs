using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Project
{
    public class ProjectDashboardDto
    {
        public string? ProjectID { get; set; }

        public string? Title { get; set; }

        public string? MajorName { get; set; }//Display MajorName 

        public string? MajorID { get; set; }

        public string? Cohort { get; set; }

        public int? MaxStudent { get; set; }

        public string? Description { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }

        public string? CreateByName { get; set; }

        public DateTime? CreateDate { get; set; }
    }
}
