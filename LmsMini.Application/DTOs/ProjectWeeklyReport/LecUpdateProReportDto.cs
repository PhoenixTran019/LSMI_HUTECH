using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectWeeklyReport
{
    public class LecUpdateProReportDto
    {
        public string? ReportID { get; set; }

        public string? ReportContent { get; set; }

        public DateTime? WeekDate { get; set; }

        public int? WeekNumber { get; set; }
    }
}
