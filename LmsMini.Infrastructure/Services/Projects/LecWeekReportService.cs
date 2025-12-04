using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ProjectWeeklyReport;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Projects
{
    public class LecWeekReportService : ILecWeekReportService
    {
        private readonly LmsDbContext _context;

        public LecWeekReportService (LmsDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateLecWeekReportAsync(CreateLecWeekReportDto dto, string lecturerId)
        {
            var mem = await _context.ProjectClassMems
                .AnyAsync(x => x.AssignId == dto.AssignId
                                && x.LecturerId == lecturerId);

            if (!mem)
            {
                throw new UnauthorizedAccessException("You do not have permission to perform this Service.");
            }

            var reportId = Uuidv7Generator.NewUuid7().ToString();

            var report = new LecturerWeeklyReport
            {
                LecReportId = reportId,
                AssigntId = dto.AssignId,
                ReportWritter = lecturerId,
                WeekNumber = dto.WeekNumber,
                WeekDate = dto.WeekDate,
                ReportContent = dto.ReportContent,
                SubmitDate = DateTime.UtcNow
            };

            _context.LecturerWeeklyReports.Add(report);

            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = lecturerId,
                DepartId = _context.StaffDeparts
                    .Where(s => s.StaffId == lecturerId)
                    .Select(s => s.DepartId)
                    .FirstOrDefault(),
                Action = "Create Weekly Report",
                TargetId = reportId,
                TargetName = _context.ProjectAssigns
                    .Where(p => p.AssignId == dto.AssignId)
                    .Select(p => p.GroupName)
                    .FirstOrDefault(),
                Timestap = DateTime.UtcNow,

            };
            
            _context.ActivityLogs.Add(log);

            await _context.SaveChangesAsync();

            return reportId;


        }
    }
}
