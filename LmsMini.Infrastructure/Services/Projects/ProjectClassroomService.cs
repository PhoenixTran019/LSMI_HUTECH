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

        //==========SERVICE TO CREATE CLASSROOM FOR THAT PROJECT==========
        public async Task<string> CreateProjectClassroomAsync(CreateProjectClassroomDto dto, string staffId)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            if (string.IsNullOrWhiteSpace(dto.ClassroomName))
                throw new ArgumentException("ClassroomName không được để trống", nameof(dto.ClassroomName));

            if (string.IsNullOrWhiteSpace(staffId))
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
                    LecturerId = staffId,
                    RoleInClass = "Lecturer"
                };

                await _context.ProjectClassMems.AddAsync(member);

                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = dto.CreatorLecturerID,
                    DepartId = _context.StaffDeparts
                            .Where(s => s.StaffId == staffId)
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

        //==========SERVICE TO TAKE ALL PROJECT CLASSROOM==========
        public async Task<List<MyClassroomDto>> GetMyClassroomsAsync(string staffId)
        {
            var query =
                from cm in _context.ProjectClassMems
                join cl in _context.ProjectClassrooms
                    on cm.ProClassId equals cl.ProClassId
                join pj in _context.Projects
                    on cl.ProjectId equals pj.ProjectId

                //Find lecturer for each classroom
                join lec in
                    (
                        from m in _context.ProjectClassMems
                        join s in _context.DepartmentStaffs on m.LecturerId equals s.StaffId
                        where m.RoleInClass == "Lecturer"
                        select new
                        {
                            m.ProClassId,
                            LecturerName = s.LastName
                        }
                    )
                on cl.ProClassId equals lec.ProClassId

                //Filler classroom this staff joining
                where cm.LecturerId == staffId

                select new MyClassroomDto
                {
                    ProClassID = cl.ProClassId,
                    ClassroomName = cl.ClassroomName,
                    ProjectTitle = pj.Title,
                    LecturerName = lec.LecturerName
                };

            return await query.Distinct().ToListAsync();
        }

        //==========SERVICE TO ADD LECTURER INTO CLASSROOM==========
        //-----ADD NEW LECTURER TO CLASSROOM
        //-----ONLY MEMBER ROLEINCLASS = LECTURER CAN ADD
        public async Task AddMemberAsync(AddLecturerToProjectClassroomDto dto, string staffId)
        {

            if (string.IsNullOrWhiteSpace(dto.ProClassID))
                throw new ArgumentException("ProClassId is null or error.", nameof(dto.ProClassID));

            if (string.IsNullOrWhiteSpace(dto.LecturerID))
                throw new ArgumentException("LecturerId is null or error.", nameof(dto.LecturerID));

            if (string.IsNullOrWhiteSpace(dto.NewMemberLecturerID))
                throw new ArgumentException("LecturerId is null or error.", nameof(dto.LecturerID));

            //Take classroom
            var classroom = await _context.ProjectClassrooms
                .FirstOrDefaultAsync(c => c.ProClassId == dto.ProClassID);
            if (classroom == null)
                throw new InvalidOperationException("Project Classroom is not exists or error!");

            //Check role of people who add
            var isLecturer = await _context.ProjectClassMems
                .AnyAsync(m => m.ProClassId == dto.ProClassID
                && m.LecturerId == staffId
                && m.RoleInClass == "Lecturer");

            if (!isLecturer)
                throw new UnauthorizedAccessException("Bạn không có quyền thêm thành viên vào lớp này.");

            //Check the user is exists
            var exists = await _context.ProjectClassMems
                .AnyAsync(m => m.ProClassId == dto.ProClassID
                            && m.LecturerId == dto.NewMemberLecturerID);

            if (exists)
                throw new InvalidOperationException("That member are in class now");

            var proClassMemID = Uuidv7Generator.NewUuid7().ToString();

            //Write DB recode
            var newMember = new ProjectClassMem
            {
                ProClassMemId = proClassMemID,
                ProClassId = dto.ProClassID,
                LecturerId = dto.NewMemberLecturerID,
                RoleInClass = "Lecturer"
            };

            await _context.ProjectClassMems.AddAsync(newMember);

            //Write Log
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = dto.LecturerID,
                DepartId = _context.StaffDeparts
                    .Where(s => s.StaffId == dto.LecturerID)
                    .Select(s => s.DepartId)
                    .FirstOrDefault(),
                Action = "Add New Lecturers",
                TargetTable = "ProjectClassMem",
                TargetId = proClassMemID,
                TargetName = dto.NewMemberLecturerID,
                Timestap = DateTime.UtcNow
            };

            await _context.ActivityLogs.AddAsync(log);

            await _context.SaveChangesAsync();
        }

        public async Task<ProjectClassroomDashboardDto> GetDashboardAsync(string proClassId)
        {
            if (string.IsNullOrWhiteSpace(proClassId))
                throw new ArgumentException("proClassId is required");

            //take infor classroom
            var classroom = await _context.ProjectClassrooms
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.ProClassId == proClassId);

            if (classroom == null)
                throw new InvalidOperationException("Project classroom not found");

            //Take content list 
            var contents = await _context.ProjectContents
                .Where(c => c.ProClassId == proClassId)
                .OrderByDescending(c => c.CreateDate)
                .Select(c => new ProjectContentItemDto
                {
                    ProContentID = c.ProContentId,
                    Title = c.Title,
                    ContentType = c.ContentType,
                    CreatedDate = c.CreateDate,

                    //If it's Assignment -> have Deadline
                    Deadline = (c.ContentType == "AssignmentLate"
                                || c.ContentType == "AssignmentNoLate")
                                ? c.Deadline
                                : null,
                    //Take poster name
                    PostedByName = _context.DepartmentStaffs
                        .Where(s => s.StaffId == c.PostedBy)
                        .Select(s => s.LastName)
                        .FirstOrDefault() ?? "Unknown"
                })
                .ToListAsync();

            return new ProjectClassroomDashboardDto
            {
                ProClassID = classroom.ProClassId,
                ClassroomName = classroom.ClassroomName,
                ProjectID = proClassId,
                InviteCode = classroom.InviteCode,
                CreateDate = classroom.CreateDate,

                Contents = contents
            };
        }

    }
}
