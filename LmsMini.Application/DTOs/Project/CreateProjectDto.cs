using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Project
{
    public class CreateProjectDto
    {

        public string? Title { get; set; }

        public string? ProMajor { get; set; }

        public string? Cohort { get; set; }

        public string? Description { get; set; }

        public int? MaxStudent { get; set; }

        public DateTime? StartDate { get; set; }

        public DateTime? EndDate { get; set; }


    }
}
