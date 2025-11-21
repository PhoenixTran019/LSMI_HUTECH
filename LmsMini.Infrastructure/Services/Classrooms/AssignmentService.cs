using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ClassAssignment;
using LmsMini.Application.DTOs.StudentClassroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Classrooms
{
    public class AssignmentService :IAssigmentService
    {
        public readonly LmsDbContext _context;

        public AssignmentService(LmsDbContext context)
        {
            _context = context;
        }

        //Service to create a new assignment with file uploads
        public async Task<string> CreateAssignmentWithFilesAsync(CreateAssigmentWithFilesDto dto, string teacherId, string webRootPath)
        {
            var assignId = Uuidv7Generator.NewUuid7().ToString();

            //Nonmalize the title to avoid issues with file paths
            var safeClassName = SlugHelper.Sluggify(dto.ClassName);
            var safeTitle = SlugHelper.Sluggify(dto.Title);

            var folderPath = Path.Combine(webRootPath, "uploads", "Assignments", safeClassName, safeTitle);
            Directory.CreateDirectory(folderPath);


            var assignment = new Assignment
            {
                AssignId = assignId,
                ClassroomId = dto.ClassrooomID,
                TeacherId = teacherId,
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                DeadlineStatus = dto.Deadline > DateTime.UtcNow ? "Valid" : "Overdue",
                HomeworkStatus = dto.HomeworkStatus,
                CreateAt = DateTime.UtcNow,
            };

            await _context.Assignments.AddAsync(assignment);

            if (dto.Files != null && dto.Files.Any())
            {
                

                foreach (var file in dto.Files)
                {
                    if (file == null|| file.Length == 0)
                        continue;


                    var originalFileName = Path.GetFileName(file.FileName);
                    var storageFileName = $"{Uuidv7Generator.NewUuid7()}_{SlugHelper.Sluggify(originalFileName)}";
                    var fullPath = Path.Combine(folderPath, storageFileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create)) 
                    
                    await file.CopyToAsync(stream);
                    

                    var relativePath = "/" + Path.Combine("uploads", "Assignments", safeClassName, safeTitle, storageFileName)
                        .Replace('\\', '/'); // For URL use forward slashes

                    var assignmentFile = new AssignmentFile
                    {
                        FileId = Uuidv7Generator.NewUuid7().ToString(),
                        AssignId = assignId,
                        FileName = originalFileName,
                        FilePath = relativePath,
                        FileType = file.ContentType,
                    };
                    await _context.AssignmentFiles.AddAsync(assignmentFile);

                    await _context.ActivityLogs.AddAsync(new ActivityLog
                    {
                        LogId = Uuidv7Generator.NewUuid7().ToString(),
                        StaffId = teacherId,
                        Action = "Create Assignment",
                        TargetId = assignId,
                        TargetTable = "Assignments",
                        TargetName = dto.Title,
                        Timestap = DateTime.UtcNow
                    });
                }
            }
            await _context.SaveChangesAsync();
            return assignId;
        }

        ///<summary>
        ///Lấy chi tiết bài tập cho giảng viên/giáo viên
        ///Bao gồm thông tin bài tập và các tệp đính kèm
        ///</summary>
        public async Task<StaffAssignmentDetailDto?> StaffGetAssignmentDetailAysnc(string assigmentId)
        {
            //Load assignment DB
            var assignmentData = await _context.Assignments
                .Where(a => a.AssignId == assigmentId)
                .Select(a => new
                {
                    a.AssignId,
                    a.Title,
                    a.Description,
                    a.Deadline,
                    a.DeadlineStatus,
                    a.HomeworkStatus,
                    ClassroomId = a.ClassroomId,
                })
                .FirstOrDefaultAsync();

            if (assignmentData == null) return null;

            var files = await _context.AssignmentFiles
                .Where(f => f.AssignId == assigmentId)
                .Select(f => new AssigmentFileDto
                {
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType
                })
                .ToListAsync();

            var classroomMembers = await _context.ClassroomMembers
                .Where(cm => cm.ClassroomId == assignmentData.ClassroomId && cm.RoleInClass == "Student")
                .ToArrayAsync();

            var studentIds = classroomMembers.Select(cm => cm.StudentId).ToList();

            var students = await _context.Students
                .Where(s => studentIds.Contains(s.StudentId))
                .ToDictionaryAsync(s => s.StudentId, s => s .FirstName + " " + s.LastName);

            var submissions = await _context.Submissions
                .Where(sub => sub.AssignId == assigmentId && studentIds.Contains(sub.StudentId))
                .GroupBy(sub => sub.StudentId)
                .ToDictionaryAsync(
                    g => g.Key,
                    g => g.OrderByDescending(sub => sub.SubmitAt).FirstOrDefault()
                );

            //Map data to DTO
            var submitList = classroomMembers.Select(cm =>
            {
                submissions.TryGetValue(cm.StudentId, out var sub);
                students.TryGetValue(cm.StudentId, out var fullname);

                return new StudentSubmitDto
                {
                    StudentId = cm.StudentId!,
                    FullName = fullname ?? cm.StudentId,
                    IsSubmitted = sub != null,
                    SubmittedAt = sub?.SubmitAt,
                    IsLate = sub != null && sub.SubmitAt > assignmentData.Deadline,
                };
            }).ToList();

            //Create final DTO
            var resultDto = new StaffAssignmentDetailDto
            {
                AssignID = assignmentData.AssignId,
                Title = assignmentData.Title,
                Description = assignmentData.Description,
                Deadline = assignmentData.Deadline,
                DeadlineStatus = assignmentData.DeadlineStatus,
                HomeworkStatus = assignmentData.HomeworkStatus,
                Files = files,
                Submit = submitList
            };

            return resultDto;

        }


    }
}
