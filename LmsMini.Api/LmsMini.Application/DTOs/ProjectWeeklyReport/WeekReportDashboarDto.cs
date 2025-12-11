using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectWeeklyReport
{
    public class WeekReportDashboarDto
    {
        public string ReportID { get; set; }
        public string LecReportID { get; set; }

        public string? AssignID { get; set; }

        public string? GroupName { get; set; }

        public string? WriterName { get; set; }

        public DateTime? WeekDate { get; set; }

        public DateTime? SubmitData { get; set; }
    }
}