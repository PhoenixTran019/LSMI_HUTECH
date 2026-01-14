using LmsMini.Application.DTOs.ProjectWeeklyReport;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface ILecWeekReportService
    {
        Task<string>CreateLecWeekReportAsync(CreateLecWeekReportDto dto, string lecturerId);

        Task<List<WeekReportDashboarDto>> GetWeekReportDashboardAsync(string username, string personType, string role);

        Task<WeekReportDetailDto> GetWeekReportDetailAsync(string reportId);

        Task UpdateLecProWeekReportAsync(LecUpdateProReportDto dto, string lecturerId);

        Task DeleteWeeklyReport(string reportId, string userId, string role);
    }
}
