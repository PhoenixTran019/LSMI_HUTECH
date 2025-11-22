using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ClassAssignment;
using LmsMini.Application.DTOs.StudentClassroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Classrooms
{
    public class AssignmentService :IAssigmentService
    {
        private readonly LmsDbContext _context;
        private readonly ILogger<AssignmentService> _logger;

        public AssignmentService(LmsDbContext context, ILogger<AssignmentService> logger)
        {
            _context = context;
            _logger = logger;
        }

        //==========Service to create a new assignment with file uploads==========
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

        //==========Service to get assignment detail for Staff/Lecturer/Admin==========
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


        //==========Service to update an existing assignment==========
        public async Task<bool> UpdateAssignmentAsync(string assignmentId, UpdateAssignmentDto dto, string staffId, string webRootPath)
        {
            //Load assignment and File
            var assigment = await _context.Assignments
                .Include(a => a.AssignmentFiles)
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId);

            if (assigment == null)
                return false;

            //Begin transaction to keep consistency
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                //update basic fields
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    assigment.Title = dto.Title.Trim();

                if(!string.IsNullOrWhiteSpace(dto.Description))
                    assigment.Description = dto.Description.Trim();

                if (dto.Deadline.HasValue)
                {
                    assigment.Deadline = dto.Deadline.Value;
                    assigment.DeadlineStatus = assigment.Deadline > DateTime.UtcNow ? "Valid" : "Overdue";
                }

                if (!string.IsNullOrWhiteSpace(dto.HomeworkStatus))
                    assigment.HomeworkStatus = dto.HomeworkStatus.Trim();

                //Build safe folder names (based on ClassName and Title)
                var classDisplay = !string.IsNullOrWhiteSpace(dto.ClassName) ? dto.ClassName : null;
                var titleDisplay = !string.IsNullOrWhiteSpace(dto.AssigmentName) ? dto.AssigmentName : null;

                string classroomNameFromDb = null;
                try
                {
                    var classroom = await _context.Classrooms.FirstOrDefaultAsync(c => c.ClassroomId == assigment.ClassroomId);
                    classroomNameFromDb = classroom?.ClassName;
                }
                catch { }

                var classForFolder = classDisplay ?? classroomNameFromDb ?? assigment.ClassroomId ?? "unknow_class";
                var titleForFolder = titleDisplay ?? assigment.Title ?? "unknow_assignment";

                var safeClassName = SlugHelper.Sluggify(classForFolder);
                var safeTitle = SlugHelper.Sluggify(titleForFolder);

                var folderRel = Path.Combine("uploads", "Assignments", safeClassName, safeTitle);
                var folderFull = Path.Combine(webRootPath, folderRel);

                Directory.CreateDirectory(folderFull);

                // 3. If rename (old folder != new folder) -> move existing physical files and update file paths
                // Build old folder path based on current DB values (before changes)
                var oldClassName = classroomNameFromDb ?? assigment.ClassroomId ?? "unknow_class";
                var oldTitleName = assigment.Title ?? "unknow_assignment";

                foreach( var file in assigment.AssignmentFiles.ToList())
                {
                    try
                    {
                        var storedPath = (file.FilePath ?? "").TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                        var currentPhysical =Path.Combine(webRootPath, storedPath);

                        // target file name remains same stored file name (the DB may contain storedFileName)
                        var storedFileName = Path.GetFileName(currentPhysical);
                        var newPhysicalPath = Path.Combine(folderFull, storedFileName);

                        //Move if file exists and paths are different
                        if(File.Exists(currentPhysical) && !string.Equals(currentPhysical, newPhysicalPath, StringComparison.OrdinalIgnoreCase))
                        {
                            //Ensure target directory exists
                            Directory.CreateDirectory(Path.GetDirectoryName(newPhysicalPath)!);
                            File.Move(currentPhysical, newPhysicalPath);

                            // Update DB FilePath
                            file.FilePath = "/" + Path.Combine(folderRel, storedFileName).Replace("\\", "/");
                           
                        }

                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error moving file for assignment {AssignmentId}", assignmentId);
                        // Continue moving other files
                    }
                }

                // 4. Remove requested files
                if(dto.RemoveFileName != null && dto.RemoveFileName.Any())
                {
                    var removeList = assigment.AssignmentFiles
                        .Where(f => dto.RemoveFileName.Contains(f.FileName))
                        .ToList();

                    foreach( var f in removeList)
                    {
                        try
                        {
                            var rel = (f.FilePath ?? "").TrimStart('/', '\\').Replace('/', Path.DirectorySeparatorChar);
                            var physicalPath = Path.Combine(webRootPath, rel);

                            if (File.Exists(physicalPath))
                                File.Delete(physicalPath);

                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error deleting physical file for assignment {AssignmentId}", assignmentId);
                        }

                        _context.AssignmentFiles.Remove(f);
                    }
                }

                //Add new files
                if(dto.NewFiles != null && dto.NewFiles.Any())
                {
                    foreach (var file in dto.NewFiles)
                    {
                        if (file == null || file.Length == 0) continue;

                        var originalName = Path.GetFileName(file.FileName);
                        var safeOriginal = SlugHelper.Sluggify(originalName);

                        var fileId = Uuidv7Generator.NewUuid7().ToString();
                        var storedFileName = $"{fileId}_{safeOriginal}";
                        var destFull = Path.Combine(folderFull, storedFileName);

                        //Save physical file
                        using (var stream = new FileStream(destFull, FileMode.Create))
                            await file.CopyToAsync(stream);

                        var relativePath = "/" + Path.Combine(folderRel, storedFileName).Replace("\\", "/");

                        var assignmentFile = new AssignmentFile
                        {
                            FileId = fileId,
                            AssignId = assignmentId,
                            FileName = originalName,
                            FilePath = relativePath,
                            FileType = file.ContentType,
                        };

                        await _context.AssignmentFiles.AddAsync(assignmentFile);
                    }
                }

                //Write activity log
                try
                {
                    var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.StaffId == staffId);
                    
                    var log = new ActivityLog
                    {
                        LogId = Uuidv7Generator.NewUuid7().ToString(),
                        StaffId = staffId,
                        
                        Action = "Update Assignment",
                        TargetId = assignmentId,
                        TargetTable = "Assignments",
                        TargetName = assigment.Title,
                        Timestap = DateTime.UtcNow
                    };

                    await _context.ActivityLogs.AddAsync(log);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error logging activity for assignment update {AssignmentId}", assignmentId);
                }

                //Save and commit
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating assignment {AssignmentId}", assignmentId);
                try { await tx.RollbackAsync(); } catch { }
                throw;
            }
        }


        //===========SERVICES FOR DELETE ASSIGNMET==========
        public async Task<bool> DeleteAssignmentAsync(string assignmentId, string staffId, string webRootPath)
        {

            //Load assignment with files
            var assignment = await _context.Assignments
                .Include(a => a.AssignmentFiles)
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId);

            if (assignment == null)
                return false;

            //Transaction -> ensuers no partial delete
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                //===Delete physical files ===
                foreach (var file in assignment.AssignmentFiles)
                {
                    try
                    {
                        var rePath = (file.FilePath ?? "")
                            .TrimStart('/', '\\')
                            .Replace('/', Path.DirectorySeparatorChar);

                        var physicalPath = Path.Combine(webRootPath, rePath);

                        if (File.Exists(physicalPath))
                            File.Delete(physicalPath);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Error deleting physical file for assignment {AssignmentId}", assignmentId);
                    }
                }
                //===Delete Folder if empty===
                try
                {
                    var className = SlugHelper.Sluggify(
                        _context.Classrooms
                        .Where(c => c.ClassroomId == assignment.ClassroomId)
                        .Select(c => c.ClassName)
                        .FirstOrDefault()
                        ?? "Unknow_Class"
                    );

                    var folderTitle = SlugHelper.Sluggify(assignment.Title ?? "unknow_assignment");

                    var assignFolder = Path.Combine(webRootPath, "uploads", "Assignments", className, folderTitle);

                    if (Directory.Exists(assignFolder))
                    {
                        Directory.Delete(assignFolder, recursive: true);
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error deleting assignment folder for assignment {AssignmentId}", assignmentId);
                }
                //===Delete DB records===
                _context.AssignmentFiles.RemoveRange(assignment.AssignmentFiles);
                _context.Assignments.Remove(assignment);

                //===Log activity===
                try
                {
                    var log = new ActivityLog
                    {
                        LogId = Uuidv7Generator.NewUuid7().ToString(),
                        StaffId = staffId,
                        Action = "Delete Assignment",
                        TargetId = assignmentId,
                        TargetTable = "Assignments",
                        TargetName = assignment.Title,
                        Timestap = DateTime.UtcNow
                    };
                    await _context.ActivityLogs.AddAsync(log);

                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error logging activity for assignment deletion {AssignmentId}", assignmentId);
                }

                //===Save and commit===
                await _context.SaveChangesAsync();
                await tx.CommitAsync();

                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting assignment {AssignmentId}", assignmentId);
                try { await tx.RollbackAsync(); } catch { }
                throw;

            }
        }
    }
}
