using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ProjectClassroom;
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
    public class ProWeeklyMeeting : IProWeeklyService
    {
        private readonly LmsDbContext _context;

        public ProWeeklyMeeting (LmsDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateMeetingAsync(string proClassId, CreateMeetingDto dto, string staffId)
        {
            var meetingId = Uuidv7Generator.NewUuid7().ToString();

            //Create new meeting
            var meeting = new WeeklyMeeting
            {
                MeetingId = meetingId,
                ProClassId = proClassId,
                LecturerId = staffId,
                MeetingDate = dto.MeetingDate,
                ProLinkMeeting = dto.ProLinkMeeting,
                Notes = dto.Notes
            };
            await _context.WeeklyMeetings.AddAsync(meeting);

            //Write log 
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffId,
                DepartId = _context.StaffDeparts
                        .Where(s => s.StaffId == staffId)
                        .Select(s => s.DepartId)
                        .FirstOrDefault(),
                Action = "Create Weekly Meeting",
                TargetTable = "WeeklyMeetings",
                TargetId = meetingId,
                TargetName = $"Meeting for {dto.MeetingDate}",
                Timestap = DateTime.UtcNow
            };

            await _context.ActivityLogs.AddAsync(log);

            await _context.SaveChangesAsync();

            return meetingId;
        }
    }
}
