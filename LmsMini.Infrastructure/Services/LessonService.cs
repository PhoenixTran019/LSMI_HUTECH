using LmsMini.Application.Common.Helpers;
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

        public async Task<bool> UpdateLessonAsync(string lessonId, UpdateLessonDto dto, string staffId, string webRootPath)
        {
            //Find Lesson; Include LessonFiles to delete Record
            var lesson = await _context.Lessons
                .Include(l => l.LessonFiles)
                .FirstOrDefaultAsync(l => l.LessonId == lessonId);

            if (lesson == null) return false;

            //1. Update Title/Content if client send
            if (!string.IsNullOrWhiteSpace(dto.Title))
                lesson.Title = dto.Title.Trim();

            if (dto.Content != null)
                lesson.Content = dto.Content.Trim();

            //2.Specifies the ClassName/LessonName used to save the file
            //If FE does not send, use current data (if available). If not, use safe defaults.
            var classNameForPath = !string.IsNullOrWhiteSpace(dto.Classname) ? dto.Classname.Trim() : (lesson.ClassroomId ?? "NoClass");
            var lessonNameForPath = !string.IsNullOrWhiteSpace(dto.LessonName) ? dto.LessonName.Trim() : (lesson.Title ?? "NoLesson");

            //standardize folders(Eliminate harmful words by replacing them with spaces.)
            string NormalizeForPath(string input)
            {
                var invalid = Path.GetInvalidFileNameChars();
                var sb = new StringBuilder();
                foreach (var ch in input)
                {
                    if (invalid.Contains(ch)) sb.Append('_');
                    else sb.Append(ch);
                }
                return sb.ToString().Replace(' ', '_');
            }

            var safeClassName = NormalizeForPath(classNameForPath);
            var safeLessonName = NormalizeForPath(lessonNameForPath);

            //Forder relative
            var folderRel = Path.Combine("uploads", "Lessons", safeClassName, safeLessonName);
            var folderFull = Path.Combine(webRootPath, folderRel);

            //Create new folder if folder doesn't esixt
            Directory.CreateDirectory(folderFull);

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
                        var rel = (fileRec.FilePath ?? "").TrimStart('/', '\\');
                        if (!string.IsNullOrEmpty(rel))
                        {
                            var phisical = Path.Combine(webRootPath, rel.Replace('/', Path.DirectorySeparatorChar));
                            if (System.IO.File.Exists(phisical))
                            {
                                System.IO.File.Delete(phisical); //Delete physical file
                            }
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
                    var normalizedOriginal = NormalizeForPath(originalFileName);

                    //Stored file name to avoid duplicates
                    var filesId = Uuidv7Generator.NewUuid7().ToString();
                    var storedFileName = $"{filesId}_{normalizedOriginal}";

                    var destFullPath = Path.Combine(folderFull, storedFileName);

                    //Save phisical file
                    using (var stream = new FileStream(destFullPath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }
                    //Save record into DB
                    var relativePathForDb = "/" + Path.Combine(folderRel, storedFileName).Replace('\\', '/');

                    var lessonFile = new LessonFile
                    {
                        FilesId = filesId,
                        LessonId = lesson.LessonId,
                        FileName = originalFileName, //Name display to FE
                        FilePath = relativePathForDb, //Link
                        FileType = file.ContentType,
                        UpdateAt = DateTime.UtcNow,
                    };
                    await _context.LessonFiles.AddAsync(lessonFile);
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
    }
}
