using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.Project;
using LmsMini.Infrastructure.Services;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using LmsMini.Application.Interfaces;

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

            var project = new Domain.Models.Project
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

        //==========SERVICE TO APPROVE==========
        public async Task<bool> ApproveAsync(ProjectApprovalDto dto, string approverId)
        {
            var assign = await _context.ProjectAssigns
                .FirstOrDefaultAsync(x => x.AssignId == dto.AssignID);

            if (assign == null)
                throw new Exception("Project Assign not found");

            var approver = await _context.DepartmentStaffs
                .FirstOrDefaultAsync(x => x.StaffId == approverId);

            if (approver == null)
                throw new Exception("Approver not found");

            bool isIndustryLeader = approver.Description == "IndustryLeader";

            //===== Rule 1: Comments required only when Reject=====
            if (dto.Decicion == "Reject" && string.IsNullOrWhiteSpace(dto.Comments))
                throw new Exception("Comments is required when Rejecting");

            //===== Rule 2: When Approve -> clear Comments =====

            //Write Approve
            var approval = new ProjectApproval
            {
                ApprovalId = Uuidv7Generator.NewUuid7().ToString(),
                RegistId = assign.AssignId,
                ApproverId = approverId,
                Decision = dto.Decicion ?? "Approve",
                Comments = dto.Comments,
                ApprovalDate = DateTime.UtcNow,
            };

            _context.ProjectApprovals.Add(approval);

            //Reject case
            if (dto.Decicion == "Reject")
            {
                assign.Status = "Rejected";
                assign.ApprovalNote = dto.Comments; //Only in reject
                await _context.SaveChangesAsync();
                return true;
            }

            //Staff approval -> move to IndustryLeader;
            if (!isIndustryLeader)
            {
                assign.Status = "PandingIndustryLeader";
                assign.ApprovalNote = null; //no comment
                await _context.SaveChangesAsync();
                return true;
            }

            if (isIndustryLeader)
            {
                if (string.IsNullOrEmpty(dto.LecturerID))
                    throw new Exception("LecturerId is required for IndustryLeader approval.");

                assign.LecturerId = dto.LecturerID;
                assign.Status = "Approved";
                assign.ApprovalNote = null; 
            }

            await _context.SaveChangesAsync();
            return true;

        }

    }
}
