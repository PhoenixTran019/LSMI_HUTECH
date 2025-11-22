using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.Project;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services
{
    public class ProjectService : IProjectService
    {
        private readonly LmsDbContext _context;
        private readonly ILogger<ProjectService> _logger;

        public ProjectService(LmsDbContext context, ILogger<ProjectService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //==========CREATE PROJECT==========
        public async Task<string> CreateProjectAsync(CreateProjectDto dto, string staffId)
        {

            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            //-----Validate StartDate and EndDate
            if (dto.StartDate >= dto.EndDate)
                throw new Exception("Start date must be earlier than end date.");

            //-----Check Major exists
            var majorEntity = await _context.Majors
                .FirstOrDefaultAsync(m => m.MajorId == dto.ProMajor);

            if (majorEntity == null) 
                throw new ArgumentException("Invalid Major: ProMajor does not exist");

            //Validate MaxStudent
            if (dto.MaxStudents <= 0)
                throw new ArgumentException("Max Student Must be greater than 0");

            var projectId = Uuidv7Generator.NewUuid7().ToString();

            var project = new Project
            {
                ProjectId = projectId,
                Title = dto.Title,
                ProMajor = majorEntity.MajorId, //Save real ID
                Cohort = dto.Cohort,
                MaxStudents = dto.MaxStudents,
                Description = dto.Description,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                CreateBy = staffId,
                CreateDate = DateTime.UtcNow,
            };

            _context.Projects.Add(project);

            //-----Write log
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffId,
                DepartId = _context.StaffDeparts
                        .Where(s => s.StaffId == staffId)
                        .Select(s => s.DepartId)
                        .FirstOrDefault(),
                Action = "Create Project Announcement",
                TargetTable = "Project",
                TargetId = projectId,
                TargetName = dto.Title,
                Timestap = DateTime.UtcNow
            };
            await _context.ActivityLogs.AddAsync(log);

            await _context.SaveChangesAsync();

            return projectId;
        }

    }
}
