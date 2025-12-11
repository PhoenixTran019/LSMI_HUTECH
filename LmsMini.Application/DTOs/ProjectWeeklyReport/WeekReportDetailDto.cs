using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectWeeklyReport
{
    public class WeekReportDetailDto
    {
        public string? ReportID { get; set; }

        public string? AssignID { get; set; }

        public string? WriteBy { get; set; }

        public string? GroupName { get; set; }

        public int? WeekNumber { get; set; }

        public DateTime? WeekDate { get; set; }

        public string? ReportContent { get; set; }

        public DateTime? SubmitDate { get; set; }
    }
}