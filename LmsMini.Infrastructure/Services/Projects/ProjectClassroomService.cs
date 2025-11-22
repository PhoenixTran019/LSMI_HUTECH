using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ProjectClassroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Project
{
    public class ProjectClassroomService : IProjectClassroomService
    {
        private readonly LmsDbContext _context;
        
        public ProjectClassroomService(LmsDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateProjectClassroomAsync(CreateProjectClassroomDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ClassroomName))
                throw new ArgumentException("ClassroomName không được để trống", nameof(dto.ClassroomName));

            if (string.IsNullOrWhiteSpace(dto.CreatorLecturerID))
                throw new ArgumentException("CreatorLecturerId không được để trống", nameof(dto.CreatorLecturerID));

            // Sử dụng transaction để đảm bảo atomic
            await using var transaction = await _context.Database.BeginTransactionAsync();
            var projectEntity = await _context.Projects
                .FirstOrDefaultAsync(p => p.ProjectId == dto.ProjectID);

            try
            {
                //1. Create ID and InviteCode
                var proClassId = Uuidv7Generator.NewUuid7().ToString();
                var inviteCode = InviteCodeGenerator.GenerateInviteCode(8);

                //Create recode for ProjectClassroom
                var classroom = new ProjectClassroom
                {
                    ProClassId = proClassId,
                    ProjectId = projectEntity.ProjectId,
                    ClassroomName = dto.ClassroomName,
                    InviteCode = inviteCode,
                    CreateDate = DateTime.UtcNow,
                    IsActive = true
                };

                await _context.ProjectClassrooms.AddAsync(classroom);

                var member = new ProjectClassMem
                {
                    ProClassMemId = Uuidv7Generator.NewUuid7().ToString(),
                    ProClassId = proClassId,
                    LecturerId = dto.CreatorLecturerID,
                    RoleInClass = "Lecturer"
                };

                await _context.ProjectClassMems.AddAsync(member);

                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = dto.CreatorLecturerID,
                    DepartId = _context.StaffDeparts
                            .Where(s => s.StaffId == dto.CreatorLecturerID)
                            .Select(s => s.DepartId)
                            .FirstOrDefault(),
                    Action = "Create Project Classroom",
                    TargetTable = "ProjectClassroom",
                    TargetId = proClassId,
                    TargetName = dto.ClassroomName,
                    Timestap = DateTime.UtcNow
                };
                await _context.ActivityLogs.AddAsync(log);

                //Save change
                await _context.SaveChangesAsync();

                //Commit transaction
                await transaction.CommitAsync();

                return proClassId;
            }
            catch
            {
                //Rollback automatic when using 'await uing'
                throw;
            }
        }

    }
}
