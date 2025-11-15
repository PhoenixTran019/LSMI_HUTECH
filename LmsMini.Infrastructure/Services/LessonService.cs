using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.Lesson;
using LmsMini.Application.DTOs.StudentClassroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services
{
    public class LessonService : ILessonService
    {
        private readonly LmsDbContext _context;

        public LessonService(LmsDbContext context)
        {
            _context = context;
        }

        //Service to create a new lesson with file uploads
        public async Task<string> CreateLessonWithFilesAsync(CreateLessonWithFilesDto dto, string staffId, string webRootPath)
        {
            var lessonId = Uuidv7Generator.NewUuid7().ToString();

            var lesson = new Lesson
            {
                LessonId = lessonId,
                CreateBy = staffId,
                ClassroomId = dto.ClassrooomID,
                Title = dto.LessonTitle,
                Content = dto.Content,
                CreateAt = DateTime.UtcNow,
            };

            await _context.Lessons.AddAsync(lesson);
            if (dto.Files != null && dto.Files.Any())
            {
                //Create Folder to save files: uploads/lessons/{lessonId}/{lessonTitle}
                var folderPath = Path.Combine(webRootPath, "uploads", "Lessons", dto.ClassName, dto.LessonTitle);
                Directory.CreateDirectory(folderPath);
                foreach (var file in dto.Files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fullPath = Path.Combine(folderPath, fileName);
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var relativePath = $"/uploads/Lessons/{dto.ClassName}/{dto.LessonTitle}/{fileName}";

                    var lessonFile = new LessonFile
                    {
                        FilesId = Uuidv7Generator.NewUuid7().ToString(),
                        LessonId = lessonId,
                        FileName = fileName,
                        FilePath = relativePath,
                        FileType = file.ContentType,
                        UpdateAt = DateTime.UtcNow
                    };

                    await _context.LessonFiles.AddAsync(lessonFile);

                    await _context.ActivityLogs.AddAsync(new ActivityLog
                    {
                        LogId = Uuidv7Generator.NewUuid7().ToString(),
                        StaffId = staffId,
                        Action =  "Create Lesson",
                        TargetId = lessonId,
                        TargetTable = "Lessons",
                        TargetName = dto.LessonTitle,
                        Timestap = DateTime.UtcNow
                    });
                }

            }
            await _context.SaveChangesAsync();
            return lessonId;

        }

        //Service to create a new assignment with file uploads
        public async Task<string> CreateAssignmentWithFilesAsync (CreateAssigmentWithFilesDto dto, string teacherId, string webRootPath)
        {
            var assignId = Uuidv7Generator.NewUuid7().ToString();

            var assignment = new Assignment
            {
                AssignId = assignId,
                ClassroomId = dto.ClassrooomID,
                TeacherId = teacherId,
                Title = dto.Title,
                Description = dto.Description,
                Deadline = dto.Deadline,
                //
                DeadlineStatus = dto.Deadline > DateTime.UtcNow ? "Valid" : "Overdue",
                HomeworkStatus = dto.HomeworkStatus,
                CreateAt = DateTime.UtcNow,
            };

            await _context.Assignments.AddAsync(assignment);

            if (dto.Files != null && dto.Files.Any())
            {
                var folderPath = Path.Combine(webRootPath, "uploads", "Assigments", dto.ClassrooomID, dto.Title);
                Directory.CreateDirectory(folderPath);

                foreach (var file in dto.Files)
                {
                    var fileName = Path.GetFileName(file.FileName);
                    var fullPath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var relativePath = $"/uploads/Assignments/{dto.ClassrooomID}/{dto.Title}/{fileName}";

                    var assignmentFile = new AssignmentFile
                    {
                        FileId = Uuidv7Generator.NewUuid7().ToString(),
                        AssignId = assignId,
                        FileName = fileName,
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

        // Service to get lesson details including files
        public async Task<LessonDetailDto?> GetLessonDetailAsync(string lessonId)
        {
            var lesson = await _context.Lessons
                .Where(l => l.LessonId == lessonId)
                .Select(l => new LessonDetailDto
                {
                    LessonId = l.LessonId,
                    Title = l.Title,
                    Content = l.Content,
                    CreatedAt = l.CreateAt,
                    Files = l.LessonFiles
                        .Where(f => f.LessonId == lessonId)
                        .Select(f => new LessonFileDto
                        {
                            FileName = f.FileName,
                            FilePath = f.FilePath,
                            FileType = f.FileType
                        }).ToList()
                })
                .FirstOrDefaultAsync();

            return lesson;
        }

        ///<summary>
        ///Get assignment detail for Staff/lec/admin
        ///Includes assignment information, attachments, and student submission status
        ///</summary>
        public async Task<StaffAssignmentDetailDto?> StaffGetAssignmentDetailAysnc(string assigmentId)
        {
            var assignment = await _context.Assignments
                .Where(a => a.AssignId == assigmentId)
                .Select(a => new StaffAssignmentDetailDto
                {
                    AssignID = a.AssignId,
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    DeadlineStatus = a.DeadlineStatus,
                    HomeworkStatus = a.HomeworkStatus,

                    //Get list of attached files
                    Files = _context.AssignmentFiles
                        .Where(f => f.AssignId == assigmentId)
                        .Select(f => new AssigmentFileDto
                        {
                            FileName= f.FileName,
                            FilePath = f.FilePath,
                            FileType = f.FileType
                        }).ToList(),

                    //Get List Student in classroom and submit Status
                    Submit = _context.ClassroomMembers
                        .Where (cm => cm.ClassroomId == a.ClassroomId && cm.RoleInClass == "Student")
                        .Select(cm => new StudentSubmitDto
                        {
                            StudentId = cm.StudentId,
                            //Grafting FN + LN into FullName
                            FullName = _context.Students
                                .Where(s => s.StudentId == cm.StudentId)
                                .Select(s => s.FirstName + " " + s.LastName)
                                .FirstOrDefault(),

                            //Check Student Submit or not
                            IsSubmitted = _context.Submissions
                                .Any(sub => sub.AssignId == assigmentId && sub.StudentId == cm.StudentId),

                            //Take Time submit (if have)
                            SubmittedAt = _context.Submissions
                                .Where(sub => sub.AssignId == assigmentId && sub.StudentId == cm.StudentId)
                                .Select(sub => (DateTime?)sub.SubmitAt)
                                .FirstOrDefault(),

                            //Check late submit (Check only went have assign submit)
                            IsLate = _context.Submissions
                                .Where(sub => sub.AssignId == assigmentId && sub.StudentId == cm.StudentId)
                                .Any(sub => sub.SubmitAt > a.Deadline)

                        }).ToList()

                }).FirstOrDefaultAsync();

            return assignment;
        }
    }
}
