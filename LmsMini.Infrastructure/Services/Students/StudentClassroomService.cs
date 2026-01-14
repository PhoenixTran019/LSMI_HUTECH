using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs;
using LmsMini.Application.DTOs.Common;
using LmsMini.Application.DTOs.Lesson;
using LmsMini.Application.DTOs.Student;
using LmsMini.Application.Interfaces;
using LmsMini.Application.Models;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Students
{
    public class StudentClassroomService :IStudentClassroomService
    {
        private readonly LmsDbContext _context;

        public StudentClassroomService(LmsDbContext context)
        {
            _context = context;
        }


        //==========SERVICE TO USING INVITE CODE TO JOIN TO CLASSROOM=========
        public async Task<string?> JoinClassroomByCodeAsync(string inviteCode, string studentId)
        {
            //Search Classroom by inviteCode and Status Active
            var classroom = await _context.Classrooms
                .Where(c => c.InviteCode == inviteCode && c.ClassStatus == "Active")
                .FirstOrDefaultAsync();

            if (classroom == null)
            {
                return null;
            }

            //Check student is in class
            var existingMember = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroom.ClassroomId && m.StudentId == studentId);

            if (existingMember)
            {
                //if in class return Classroom ID
                return classroom.ClassroomId;
            }

            //AddStudent in to classroom
            var newMember = new ClassroomMember
            {
                MemberId = Uuidv7Generator.NewUuid7().ToString(),
                ClassroomId = classroom.ClassroomId,
                StudentId = studentId,
                RoleInClass = "Student"
            };

            _context.ClassroomMembers.Add(newMember);

            await _context.SaveChangesAsync();

            return classroom.ClassroomId;

        }

        public async Task<List<ClassroomCardViewModel>> GetMyClassroomsAsync(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("userId is required");

            // map userId -> studentId (fallback nếu hệ đang dùng studentId == userId)
            var studentId = await _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();

            studentId ??= userId;

            var classroomIds = await _context.ClassroomMembers
                .Where(m => m.StudentId == studentId)
                .Select(m => m.ClassroomId)
                .Where(id => id != null)
                .Select(id => id!)
                .ToListAsync();

            var result = await _context.Classrooms
                .AsNoTracking()
                .Where(c => classroomIds.Contains(c.ClassroomId))
                .OrderByDescending(c => c.CreateDate)
                .Select(c => new ClassroomCardViewModel
                {
                    ClassroomId = c.ClassroomId,
                    ClassName = c.ClassName ?? "",
                    ClassSub = c.ClassSubNavigation != null ? c.ClassSubNavigation.SubName : (c.ClassSub ?? ""),
                    MainClassName = c.MainClassNavigation != null ? c.MainClassNavigation.ClassName : (c.MainClass ?? ""),
                    LecturerName = c.CreateByNavigation != null ? c.CreateByNavigation.FirstName : "",
                    ClassStatus = c.ClassStatus ?? ""
                })
                .ToListAsync();

            return result;
        }

        public async Task<StudentLessonViewDto?> GetLessonDetailAsync(string classroomId, string lessonId, string studentId)
        {
            // Kiểm tra quyền thành viên trước khi cho xem chi tiết
            var isMember = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId && m.StudentId == studentId);

            if (!isMember) return null;

            // Truy vấn chi tiết Lesson thuộc về Classroom đó
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(l => l.LessonFiles)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.ClassroomId == classroomId);

            if (lesson == null) return null;

            return new StudentLessonViewDto
            {
                LessonId = lesson.LessonId,
                Title = lesson.Title,
                Content = lesson.Content,
                CreateAt = lesson.CreateAt,
                Files = lesson.LessonFiles.Select(f => new FileDto
                {
                    FileID = f.FilesId,
                    FileName = f.FileName,
                    FileType = f.FileType
                }).ToList()
            };
        }

        public async Task<FileDownloadInfo?> GetLessonFileForDownloadAsync(string classroomId, string lessonId, string fileId, string studentId, string webRootPath)
        {
            // Kiểm tra membership
            var isMember = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId && m.StudentId == studentId);
            if (!isMember) return null;

            // Ràng buộc chặt chẽ: File thuộc Lesson, Lesson thuộc Classroom
            var fileRec = await _context.LessonFiles
                .AsNoTracking()
                .FirstOrDefaultAsync(f =>
                    f.FilesId == fileId &&
                    f.LessonId == lessonId &&
                    _context.Lessons.Any(l => l.LessonId == lessonId && l.ClassroomId == classroomId));

            if (fileRec == null || string.IsNullOrWhiteSpace(fileRec.FilePath)) return null;

            var physicalPath = Path.Combine(webRootPath, fileRec.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!System.IO.File.Exists(physicalPath)) return null;

            return new FileDownloadInfo
            {
                PhysicalPath = physicalPath,
                ContentType = "application/octet-stream",
                DownloadName = fileRec.FileName ?? Path.GetFileName(physicalPath)
            };
        }

    }
}
