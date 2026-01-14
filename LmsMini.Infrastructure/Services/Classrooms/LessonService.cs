using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ClassAssignment;
using LmsMini.Application.DTOs.Lesson;
using LmsMini.Application.DTOs.StudentClassroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services
{
    public class LessonService : ILessonService
    {
        private readonly LmsDbContext _context;
        private readonly ILogger<LessonService> _logger;

        public LessonService(LmsDbContext context, ILogger<LessonService> logger)
        {
            _context = context;
            _logger = logger;
        }
        
        //==========Service to create a new lesson with file uploads==========
        public async Task<string> CreateLessonWithFilesAsync(CreateLessonWithFilesDto dto, string staffId, string webRootPath)
        {
            if (string.IsNullOrEmpty(webRootPath))
            {
                // Ném lỗi rõ ràng thay vì để Path.Combine tự crash
                throw new InvalidOperationException("WebRootPath is missing. Please ensure the 'wwwroot' folder exists in your project root.");
            }

            //Create Lesson ID.
            var lessonId = Uuidv7Generator.NewUuid7().ToString();

            //Check classroom exists
            var classroomExists = await _context.Classrooms.AnyAsync(c => c.ClassroomId == dto.ClassrooomID);
            if (!classroomExists)
            {
                throw new ArgumentException("Classroom does not exist.");
            }

            //Clear and safe folder names
            

            //Create link folder to save file
            var folderPath = Path.Combine (webRootPath, "uploads", "Lessons", dto.ClassrooomID, lessonId);
            Directory.CreateDirectory(folderPath);

            //Start transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                //Create lesson record
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

                //Log: Create Log lesson
                await _context.ActivityLogs.AddAsync(new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = staffId,
                    DepartId = _context.StaffDeparts
                        .Where(s => s.StaffId == staffId)
                        .Select(s => s.DepartId)
                        .FirstOrDefault(),
                    Action = "Create Lesson",
                    TargetId = lessonId,
                    TargetTable = "Lessons",
                    TargetName = dto.LessonTitle,
                    Timestap = DateTime.UtcNow
                });

                //File upload if have
                if (dto.Files != null)
                {
                    foreach (var file in dto.Files)
                    {
                        //Drop file if null or empty
                        if (file == null || file.Length == 0)
                            continue;

                        var fileName = Path.GetFileName(file.FileName);
                        var fullPath = Path.Combine(folderPath, fileName);

                        //Save physical file to server
                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await file.CopyToAsync(stream);
                        }

                        //Create Path to FE access
                        var relativePath = $"/uploads/Lessons/{dto.ClassrooomID}/{lessonId}/{fileName}";

                        //Save file record to DB
                        var lessonFile = new LessonFile
                        {
                            FilesId = Uuidv7Generator.NewUuid7().ToString(),
                            LessonId = lessonId,
                            FileName = fileName,
                            FilePath = relativePath,
                            FileType = file.ContentType,
                            UpdateAt = DateTime.UtcNow,
                        };

                        await _context.LessonFiles.AddAsync(lessonFile);

                    }
                }

                //Save to DB and commit transaction
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return lessonId;
            }catch (Exception ex)
            {
                //Write log error
                _logger.LogError(ex, "Error creating lesson with files.");

                //Rollback transaction
                await transaction.RollbackAsync();
                throw;
            }

        }

        

        //==========Service to get lesson details including files==========
        public async Task<LessonDetailDto?> GetLessonDetailAsync(string classroomId, string lessonId)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(l => l.LessonFiles)
                .Where(l => l.LessonId == lessonId && l.ClassroomId == classroomId)
                .Select(l => new LessonDetailDto
                {
                    LessonId = l.LessonId,
                    ClassroomID = l.ClassroomId,
                    Title = l.Title,
                    Content = l.Content,
                    CreatedAt = l.CreateAt,

                    Files = l.LessonFiles
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

        
        //==========Serxvice to Update Lesson==========
        public async Task<bool> UpdateLessonAsync(string classroomId, string lessonId, UpdateLessonDto dto, string staffId, string webRootPath)
        {
            //Find Lesson; Include LessonFiles to delete Record
            var lesson = await _context.Lessons
                .Include(l => l.LessonFiles)
                .Include(l => l.Classroom) //Take classrom name if needed
                .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.ClassroomId == classroomId);

            if (lesson == null) return false;

            // ✅ SỬA 1: Kiểm tra null webRootPath
            if (string.IsNullOrEmpty(webRootPath))
            {
                throw new InvalidOperationException("WebRootPath is missing. Cannot perform file operations.");
            }

            //1. Update Title/Content if client send
            if (!string.IsNullOrWhiteSpace(dto.Title))
                lesson.Title = dto.Title.Trim();

            if (dto.Content != null)
                lesson.Content = dto.Content.Trim();

            var folderPath = Path.Combine(webRootPath, "uploads", "Lessons", lesson.ClassroomId, lesson.LessonId);

            //Create new folder if folder doesn't esixt
            Directory.CreateDirectory(folderPath);

            

            //Handle to delete file (base on FileName original by FE sent in RemoveFileName)
            if (dto.RemoveFileName != null && dto.RemoveFileName.Any())
            {
                //Take same record (compare FileName)
                var filesToRemove = lesson.LessonFiles
                    .Where(f => dto.RemoveFileName.Contains(f.FileName))
                    .ToList();

                foreach (var fileRec in filesToRemove)
                {
                    try
                    {
                        //create physical path from FilePath save in DB
                        var fileName = Path.GetFileName(fileRec.FilePath);

                        var phisicalPath = Path.Combine(folderPath, fileName);
                        if (File.Exists(phisicalPath))
                        {
                            File.Delete(phisicalPath); //Delete physical file
                        }
                        
                    }
                    catch (Exception ex)
                    {
                        //Error Log but no stop all flow
                        _logger.LogWarning(ex, "Error when delete physical file");
                    }

                    //Delete record DB
                    _context.LessonFiles.Remove(fileRec);
                }
            }

            //4.Handle add new file (save follow folderRel)
            if (dto.NewFiles != null && dto.NewFiles.Any())
            {
                foreach (var file in dto.NewFiles)
                {
                    if (file == null || file.Length == 0) continue;

                    //Normalize original file name to display
                    var originalFileName = Path.GetFileName(file.FileName);

                    //Stored file name to avoid duplicates
                    var filesId = Uuidv7Generator.NewUuid7().ToString();
                    var storedFileName = $"{filesId}_{originalFileName}";

                    var fullPath = Path.Combine(folderPath, storedFileName);

                    //Save phisical file
                    using (var stream = new FileStream(fullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    //Save record into DB
                    var relativePath= $"/uploads/Lessons/{lesson.ClassroomId}/{lesson.LessonId}/{storedFileName}";

                    await _context.LessonFiles.AddAsync (new LessonFile
                    {
                        FilesId = filesId,
                        LessonId = lesson.LessonId,
                        FileName = originalFileName, //Name display to FE
                        FilePath = relativePath, //Link
                        FileType = file.ContentType,
                        UpdateAt = DateTime.UtcNow,
                    });
                    
                }
            }

            //5. Write log 
            var staff = await _context.StaffDeparts
                .FirstOrDefaultAsync(s => s.StaffId == staffId);
            try
            {
                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = staffId,
                    DepartId = _context.StaffDeparts
                        .Where(s => s.StaffId == staffId)
                        .Select(s => s.DepartId)
                        .FirstOrDefault(),
                    Action = "Update Lesson",
                    TargetTable = "Lessons",
                    TargetId = lesson.LessonId,
                    TargetName = lesson.Title,
                    Timestap = DateTime.UtcNow
                };
                await _context.ActivityLogs.AddAsync(log);

            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot write Log for Update");
            }
            //6.Savechange to DB
            await _context.SaveChangesAsync();

            return true;
        }

        //==========SERVICE TO DELETE LESSON AND DELETE FILE WHEN LESSON HAS BEEN DELETE==========
        public async Task<bool> DeleteLessonAsync(string classroomId,string lessonId, string staffId, string webRootPath)
        {
            var lesson = await _context.Lessons
                .Include(l => l.LessonFiles)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.ClassroomId == classroomId);

            if (lesson == null) return false;
            if (string.IsNullOrEmpty(webRootPath))
            {
                // Nếu không có webRootPath, ta vẫn có thể xóa DB, nhưng cần Log cảnh báo
                _logger.LogWarning("WebRootPath is missing. Deleting DB records only, skipping physical file deletion for lesson {LessonId}", lessonId);
            }
            else
            {

                //Full path Folder
                var lessonFolder = Path.Combine(webRootPath, "uploads", "Lessons", classroomId, lessonId);

                //Delete physical files
                foreach (var file in lesson.LessonFiles)
                {
                    try
                    {
                        var fileName = Path.GetFileName(file.FilePath);
                        var phisicalPath = Path.Combine(lessonFolder, fileName);
                        if (File.Exists(phisicalPath))
                            File.Delete(phisicalPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deleting physical file for lesson {LessonId}", lessonId);
                    }
                }

                //If folder empty, delete folder
                try
                {
                    if (Directory.Exists(lessonFolder) && !Directory.EnumerateFileSystemEntries(lessonFolder).Any())
                    {
                        Directory.Delete(lessonFolder, true);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error deleting lesson folder for lesson {Folder}", lessonFolder);
                }
            }

            //Delete DB records
            _context.LessonFiles.RemoveRange(lesson.LessonFiles);
            _context.Lessons.Remove(lesson);

            //Write log
            try
            {
                var staff = await _context.StaffDeparts.FirstOrDefaultAsync(s => s.StaffId == staffId);

                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = staffId,
                    DepartId = staff.DepartId,
                    Action = "Delete Lesson",
                    TargetTable = "Lessons",
                    TargetId = lesson.LessonId,
                    TargetName = lesson.Title,
                    Timestap = DateTime.UtcNow
                };
                await _context.ActivityLogs.AddAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cannot write Log for Delete Lesson");
            }

            await _context.SaveChangesAsync();
            return true;
        }

    }
}
