using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.Lesson;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Http;
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

                }
            }
            await _context.SaveChangesAsync();
            return assignId;
        }
    }
}
