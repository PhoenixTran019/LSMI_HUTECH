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

        //Service to create a new lesson with file uploads
        public async Task<string> CreateLessonWithFilesAsync(CreateLessonWithFilesDto dto, string staffId, string webRootPath)
        {
            //Create Lesson ID.
            var lessonId = Uuidv7Generator.NewUuid7().ToString();

            //Check classroom exists
            var classroomExists = await _context.Classrooms.AnyAsync(c => c.ClassroomId == dto.ClassrooomID);
            if (!classroomExists)
            {
                throw new ArgumentException("Classroom does not exist.");
            }

            //Clear and safe folder names
            var safeClassName = SlugHelper.Sluggify(dto.ClassName);
            var safeLessonTitle = SlugHelper.Sluggify(dto.LessonTitle);

            //Create link folder to save file
            var folderPath = Path.Combine (webRootPath, "uploads", "Lessons", safeClassName, safeLessonTitle);
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
                        var relativePath = $"/uploads/Lessons/{safeClassName}/{safeLessonTitle}/{fileName}";

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

        

        // Service to get lesson details including files
        public async Task<LessonDetailDto?> GetLessonDetailAsync(string lessonId)
        {
            var lesson = await _context.Lessons
                .AsNoTracking()
                .Include(l => l.LessonFiles)
                .Where(l => l.LessonId == lessonId)
                .Select(l => new LessonDetailDto
                {
                    LessonId = l.LessonId,
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

        

        public async Task<bool> UpdateLessonAsync(string lessonId, UpdateLessonDto dto, string staffId, string webRootPath)
        {
            //Find Lesson; Include LessonFiles to delete Record
            var lesson = await _context.Lessons
                .Include(l => l.LessonFiles)
                .Include(l => l.Classroom) //Take classrom name if needed
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null) return false;

            //1. Update Title/Content if client send
            if (!string.IsNullOrWhiteSpace(dto.Title))
                lesson.Title = dto.Title.Trim();

            if (dto.Content != null)
                lesson.Content = dto.Content.Trim();

            //2.Specifies the ClassName/LessonName used to save the file
            //If FE does not send, use current data (if available). If not, use safe defaults.
            string originalClassName = lesson.Classroom?.ClassName ?? "NoClass";
            string originalLessonName = lesson.Title ?? "NoLesson";


            string newClassName = !string.IsNullOrWhiteSpace(dto.Classname)
                ? dto.Classname.Trim()
                : originalClassName;

            string newLessonName = !string.IsNullOrWhiteSpace(dto.LessonName)
                ?dto.LessonName.Trim()
                : originalLessonName;

            var safeOriginalClass = SlugHelper.Sluggify(originalClassName);
            var safeOriginalLesson = SlugHelper.Sluggify(originalLessonName);

            var safeNewClass = SlugHelper.Sluggify(newClassName);
            var safeNewLesson = SlugHelper.Sluggify(newLessonName);

            //Path folder old and new
            var oldFolder = Path.Combine(webRootPath, "uploads", "Lessons", safeOriginalClass, safeOriginalLesson);
            var newFolderRel = Path.Combine("uploads", "Lessons", safeNewClass, safeNewClass);
            var newFolderFull = Path.Combine(webRootPath, safeNewClass);

            //Create new folder if folder doesn't esixt
            Directory.CreateDirectory(newFolderFull);

            //If folder name change, move file to new folder
            if (safeOriginalClass != safeNewClass || safeOriginalLesson != safeNewLesson)
            {
                foreach (var f in lesson.LessonFiles)
                {
                    var oldFull = Path.Combine(webRootPath, f.FilePath.TrimStart('/', '\\'));

                    if (System.IO.File.Exists(oldFull))
                    {
                        var newFull = Path.Combine(newFolderFull, Path.GetFileName(oldFull));
                        System.IO.File.Move(oldFull, newFull);

                        //Update FilePath in DB
                        f.FilePath = "/" +Path.Combine(newFolderRel, Path.GetFileName(newFull)).Replace('\\', '/');
                        f.UpdateAt =DateTime.UtcNow;
                    }
                }
            }

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
                        var rel = fileRec.FilePath.TrimStart('/', '\\');
                        var phisical = Path.Combine(webRootPath, rel);
                        if (System.IO.File.Exists(phisical))
                        {
                            System.IO.File.Delete(phisical); //Delete physical file
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
                    var normalizedOriginal = SlugHelper.Sluggify(originalFileName);

                    //Stored file name to avoid duplicates
                    var filesId = Uuidv7Generator.NewUuid7().ToString();
                    var storedFileName = $"{filesId}_{normalizedOriginal}";

                    var destFullPath = Path.Combine(newFolderFull, storedFileName);

                    //Save phisical file
                    using (var stream = new FileStream(destFullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    //Save record into DB
                    var relativePathForDb = "/" + Path.Combine(newFolderRel, storedFileName).Replace('\\', '/');

                    await _context.LessonFiles.AddAsync (new LessonFile
                    {
                        FilesId = filesId,
                        LessonId = lesson.LessonId,
                        FileName = originalFileName, //Name display to FE
                        FilePath = relativePathForDb, //Link
                        FileType = file.ContentType,
                        UpdateAt = DateTime.UtcNow,
                    });
                    
                }
            }
            lesson.CreateAt = DateTime.UtcNow; //Update time

            //5. Write log 
            var staff = await _context.StaffDeparts
                .FirstOrDefaultAsync(s => s.StaffId == staffId);
            try
            {
                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = staffId,
                    DepartId = staff.DepartId,
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

        public async Task<bool> DeleteLessonAsync(string lessonId, string staffId, string webRootPath)
        {
            var lesson = await _context.Lessons
                .Include(l => l.LessonFiles)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null) return false;

            var safeClassName = SlugHelper.Sluggify(lesson.Classroom?.ClassName ?? "NoClass");
            var safeLessonTitle = SlugHelper.Sluggify(lesson.Title ?? "NoLesson");

            //Full path Folder
            var lessonFolder = Path.Combine(webRootPath, "uploads", "Lessons", safeClassName, safeLessonTitle);

            //Delete physical files
            foreach( var file in lesson.LessonFiles)
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
