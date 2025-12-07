using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ProjectWeeklyReport;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Mvc;
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

        //==========SERVICE TO CREATE WEEKLY REPORT==========
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
                TargetTable = "LecturerWeeklyReports",
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


        //==========SERVICE TO GET WEEKLY REPORT DASHBOARD (REPORT BY LECTURER)==========
        public async Task<List<WeekReportDashboarDto>> GetWeekReportDashboardAsync(string username, string personType, string role)
        {
            var query = _context.LecturerWeeklyReports
                .Include(r => r.Assignt)
                .AsQueryable();

            //---Student just only see they report
            if (role == "Student")
            {
                var assignIds = await _context.ProjectMenbers
                    .Where(m  => m.StudentId == username)
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

        //==========SERVICE TO TAKE REPORT DETAIL(WRITE BY LECTURER)==========
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

        //==========SEVICE TO UPDATE LECTURER WEEKLY REPORT==========
        public async Task UpdateLecProWeekReportAsync(LecUpdateProReportDto dto, string lecturerId)
        {
            var report = await _context.LecturerWeeklyReports
                .FirstOrDefaultAsync(r => r.LecReportId == dto.ReportID);

            if (report == null)
                throw new KeyNotFoundException("Report not found");

            //Check lecturer permission (Lecturer must be writer)
            if (report.ReportWritter != lecturerId)
                throw new UnauthorizedAccessException("You do not have permission");

            //Check role = Lecturer only
            var role = _context.Users
                .Where(u => u.UserId == lecturerId)
                .Select(u => u.Role.RoleName)
                .FirstOrDefault();

            if (role != "Lecturer")
                throw new UnauthorizedAccessException("Only Lecturer can update report");

            //Check SubmitDate < 14 Days
            if (report.SubmitDate.HasValue &&
                (DateTime.UtcNow - report.SubmitDate.Value).TotalDays >= 14)
            {
                throw new UnauthorizedAccessException("Update time expired (limit 14 days/2 week)");
            }

            //Update
            report.ReportContent = dto.ReportContent;
            report.WeekNumber = dto.WeekNumber;
            report.WeekDate = dto.WeekDate;

            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = lecturerId,
                DepartId = _context.StaffDeparts
                    .Where(s => s.StaffId == lecturerId)
                    .Select(s => s.DepartId)
                    .FirstOrDefault(),
                Action = "Lecturer Update Report",
                TargetTable = "LecturerWeeklyReports",
                TargetId = report.LecReportId,
                TargetName = report.ReportContent,
                Timestap = DateTime.UtcNow
            };
            _context.ActivityLogs.Add(log);

            await _context.SaveChangesAsync();

        }

        public async Task DeleteWeeklyReport(string reportId, string userId, string role)
        {
            var report = await _context.LecturerWeeklyReports
                .Include(r => r.Assignt)
                .FirstOrDefaultAsync(r => r.LecReportId == reportId);

            if (report == null)
                throw new KeyNotFoundException("Weekly report not found");

            //LECTURER case
            if (role == "Lecturer")
            {
                if (report.ReportWritter != userId)
                    throw new UnauthorizedAccessException("You do not own this report");

                // Lecturer delete allowed anytime
            }
            //ADMIN
            else if (role == "Admin")
            {
                //allow
            }
            else
            {
                throw new UnauthorizedAccessException("You don't have permission");
            }

            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = userId,
                DepartId = _context.StaffDeparts
                    .Where(s => s.StaffId == userId)
                    .Select(s => s.DepartId)
                    .FirstOrDefault(),
                Action = "Lecturer Delete Report",
                TargetTable = "LecturerWeeklyReports",
                TargetId = report.LecReportId,
                TargetName = report.ReportContent,
                Timestap = DateTime.UtcNow

            };

            _context.LecturerWeeklyReports.Remove(report);

            await _context.SaveChangesAsync();
        }

    }
}
