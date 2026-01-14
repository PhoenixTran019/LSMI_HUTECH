using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectWeeklyReport
{
    public class CreateLecWeekReportDto
    {
        public string? AssignId { get; set; } //Select from dropdown

        public int? WeekNumber { get; set; }

        public DateTime? WeekDate { get; set; }

        public string? ReportContent { get; set; }
    }
}
