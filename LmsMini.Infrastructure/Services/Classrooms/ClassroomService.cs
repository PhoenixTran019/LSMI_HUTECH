using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.Classroom;
using LmsMini.Application.Interfaces;
using LmsMini.Application.Models;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Classrooms
{
    public class ClassroomService : IClassroomService
    {
        private readonly LmsDbContext _context;

        public ClassroomService(LmsDbContext context)
        {
            _context = context;
        }

        //Service to create a new classroom
        public async Task<bool> CreateClassroomAsync (CreateClassroomDto dto, string staffId)
        {
            //Check if Classroom with the same name already exists
            if (await _context.Classrooms.AnyAsync(c => c.ClassName == dto.ClassName))
            {
                return false; // Classroom name already exists
            }

            var clasroomId = Uuidv7Generator.NewUuid7().ToString();
            var inviteCode = InviteCodeGenerator.GenerateInviteCode(8);

            //If have main class, take Course of this
            Class? mainClassEntity = null;
            if (!string.IsNullOrWhiteSpace(dto.MainClass))
            {
                mainClassEntity = await _context.Classes
                    .FirstOrDefaultAsync(c => c.ClassId == dto.MainClass);
            }

            var classroom = new Classroom
            {
                ClassroomId = clasroomId,
                ClassName = dto.ClassName,
                ClassSub = dto.ClassSub,
                MainClass = mainClassEntity?.ClassId,
                Description = dto.Description,
                InviteCode = inviteCode,
                CreateBy = staffId,
                ClassStatus = dto.ClassStatus,
            };

            _context.Classrooms.Add(classroom);

            //Add the creator to the class as Teacher
            var createrMember = new ClassroomMember
            {
                MemberId = Uuidv7Generator.NewUuid7().ToString(),
                ClassroomId = clasroomId,
                LecturerId = staffId,
                RoleInClass = "Teacher",
            };
            _context.ClassroomMembers.Add(createrMember);

            //If have main class, add the main class as a member too
            if (!string.IsNullOrEmpty(dto.MainClass))
            {
                var students = await _context.Students
                    .Where(s => s.ClassId == dto.MainClass)
                    .ToListAsync();

                foreach (var student in students)
                {
                    var member = new ClassroomMember
                    {
                        MemberId = Uuidv7Generator.NewUuid7().ToString(),
                        ClassroomId = clasroomId,
                        StudentId = student.StudentId,
                        RoleInClass = "Student",
                    };
                    _context.ClassroomMembers.Add(member);
                }
            }

            var staff = await _context.StaffDeparts
                .FirstOrDefaultAsync(s => s.StaffId == staffId);
            
            var departId = staff?.DepartId ?? "Unknown";

            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffId,
                DepartId = departId,
                Action = "Create Classroom",
                TargetTable = "Classrooms",
                TargetId = clasroomId,
                TargetName = dto.ClassName,
                Timestap = DateTime.UtcNow,
            };

            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return true;
        }

        //Service to get all classrooms
        public async Task<List<ClassroomCardViewModel>> GetDashboardClassroomsAsync(ClassroomFilterDto filter, List<string>? allowedDepartIds)
        {
            //Start the query from the Classrooms table and include related tables
            var query = _context.Classrooms
                .Include(c => c.ClassSubNavigation)
                .Include(c => c.MainClassNavigation)
                .Include(c => c.CreateByNavigation)
                .AsQueryable();

            //Search by class name
            if(!string.IsNullOrWhiteSpace(filter.Keyword))
                query = query.Where(c => c.ClassName.Contains(filter.Keyword));

            //Search by subject
            if(!string.IsNullOrWhiteSpace(filter.SubjectId))
                query = query.Where(c => c.ClassSub == filter.SubjectId);

            //Search by main class
            if(!string.IsNullOrWhiteSpace(filter.MainClassId))
                query = query.Where(c => c.MainClass == filter.MainClassId);

            //Search by course
            if(!string.IsNullOrWhiteSpace(filter.Course))
                query = query.Where(c => c.MainClassNavigation.Course == filter.Course);

            //If not admin, only get classes that on the staff manage
            if (allowedDepartIds?.Any() == true)
                query = query.Where(c => allowedDepartIds.Contains(c.ClassSubNavigation.DepartId));

            //Pagination and mapping to ViewModel
            var result = await query
                .OrderByDescending(c => c.ClassroomId)
                .Skip((filter.Page - 1) * filter.PageSize)
                .Take(filter.PageSize)
                .Select(c => new ClassroomCardViewModel
                {
                    ClassroomId = c.ClassroomId,
                    ClassName = c.ClassName,
                    ClassSub = c.ClassSubNavigation.SubName,
                    MainClassName =c.MainClassNavigation.ClassName,
                    Course = c.MainClassNavigation.Course,
                    LecturerName = c.CreateByNavigation.FirstName,
                    ClassStatus = c.ClassStatus,
                    InviteCode = c.InviteCode,
                })
                .ToListAsync();

            return result;
        }

        //Service to add member to classroom
        public async Task<bool> AddMemberToClassroomAsync (string classroomId, string userId, string role)
        {
            var classroom = await _context.Classrooms.FindAsync(classroomId);
            if (classroom == null) return false;

            bool isLecturer = role == "Teacher";

            var exists = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId &&
                                ((isLecturer && m.LecturerId == userId) || (!isLecturer && m.StudentId == userId)));

            if (exists) return false; // Member already exists -> do not add again

            var member = new ClassroomMember
            {
                MemberId = Uuidv7Generator.NewUuid7().ToString(),
                ClassroomId = classroomId,
                RoleInClass = role,
                LecturerId = isLecturer ? userId : null,
                StudentId = isLecturer ? null : userId,
            };
            await _context.ClassroomMembers.AddAsync(member);
            await _context.SaveChangesAsync();
            return true;
        }

        //Service to update role
        public async Task<bool> UpdateMemberRoleAsync (string classroomId, string userId, string newRole)
        {
            bool isLecturer = newRole == "Teacher";

            var member = await _context.ClassroomMembers
                .FirstOrDefaultAsync(m => m.ClassroomId == classroomId &&
                                          ((m.LecturerId == userId)|| (m.StudentId == userId)));

            if (member == null) return false;

            member.RoleInClass = newRole;
            member.LecturerId = isLecturer ? userId : null;
            member.StudentId = isLecturer? null : userId;

            _context.ClassroomMembers.Update(member);
            await _context.SaveChangesAsync();
            return true;
        }

        //Service to get overview of classroom
        public async Task<ClassroomOverviewDto> GetOverviewAsync (string classroomId)
        {
            var lesson = await _context.Lessons
                .Where(l => l.ClassroomId == classroomId)
                .Select(l => new LessonViewDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    CreateAt = l.CreateAt,
                }).ToListAsync();

            var assignment = await _context.Assignments
                .Where(a => a.ClassroomId == classroomId)
                .Select(a => new AssignmentViewDto
                {
                    AssignId = a.AssignId,
                    Title = a.Title,
                    Deadline = a.Deadline,
                    DeadlineStatus = a.Deadline > DateTime.UtcNow ? "Valid" : "Overdue"

                }).ToListAsync();

            return new ClassroomOverviewDto
            {
                ClassroomId = classroomId,
                Lessons = lesson,
                Assignments = assignment
            };
        }
    }
}
