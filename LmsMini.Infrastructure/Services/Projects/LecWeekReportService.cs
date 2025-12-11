using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ProjectWeeklyReport;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Domain.Entities;
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

        public LecWeekReportService(LmsDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateLecWeekReportAsync(CreateLecWeekReportDto dto, string lecturerId)
        {
            var mem = await _context.ProjectAssigns
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

        public async Task<List<WeekReportDashboarDto>> GetWeekReportDashboardAsync(string username, string personType, string role)
        {
            var query = _context.LecturerWeeklyReports
                .Include(r => r.Assignt)
                .AsQueryable();

            //---Student just only see they report
            if (role == "Student")
            {
                var assignIds = await _context.ProjectMenbers
                    .Where(m => m.StudentId == username)
                    .Select(m => m.RegistId)
                    .ToListAsync();

                query = query.Where(r => assignIds.Contains(r.AssigntId));
            }

            else if (role == "Lecturer")
            {
                //Take AssignId Lecturer
                var lecturerAssignIds = await _context.ProjectAssigns
                    .Where(a => a.LecturerId == username)
                    .Select(a => a.AssignId)
                    .ToListAsync();

                query = query.Where(r => lecturerAssignIds.Contains(r.AssigntId));
            }
            else
            {

            }

            return await query
                .Select(r => new WeekReportDashboarDto
                {
                    AssignID = r.AssigntId,
                    WriterName = r.ReportWritter,
                    WeekDate = r.WeekDate,
                    SubmitData = r.SubmitDate,
                    GroupName = r.Assignt.GroupName
                })
                .ToListAsync();
        }

        public async Task<WeekReportDetailDto> GetWeekReportDetailAsync(string reportId)
        {
            //Find report by ID
            var report = await _context.LecturerWeeklyReports
                .Include(r => r.Assignt)
                .FirstOrDefaultAsync(r => r.LecReportId == reportId);

            if (report == null)
                throw new KeyNotFoundException("Report Not Found");

            return new WeekReportDetailDto
            {
                ReportID = report.LecReportId,
                AssignID = report.AssigntId,
                WriteBy = report.ReportWritter,
                GroupName = report.Assignt.GroupName,
                WeekNumber = report.WeekNumber,
                WeekDate = report.WeekDate,
                ReportContent = report.ReportContent,
                SubmitDate = report.SubmitDate
            };
        }
    }
}