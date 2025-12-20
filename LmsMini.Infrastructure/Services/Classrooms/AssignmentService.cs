using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.ClassAssignment;
using LmsMini.Application.DTOs.Common;
using LmsMini.Application.DTOs.StudentClassroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Logging;
using Serilog.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Classrooms
{
    public class AssignmentService : IAssigmentService
    {
        private readonly LmsDbContext _context;
        private readonly ILogger<AssignmentService> _logger;
        private readonly IWebHostEnvironment _env;

        public AssignmentService(LmsDbContext context, ILogger<AssignmentService> logger, IWebHostEnvironment env)
        {
            _context = context;
            _logger = logger;
            _env = env;
        }

        //==========Service to create a new assignment with file uploads==========
        public async Task<string> CreateAssignmentWithFilesAsync(CreateAssigmentWithFilesDto dto, string teacherId, string webRootPath)
        {
            // 1. Kiểm tra WebRootPath để tránh crash 500 như phần Lesson
            if (string.IsNullOrEmpty(webRootPath))
            {
                throw new InvalidOperationException("WebRootPath is missing. Ensure 'wwwroot' folder exists.");
            }

            var assignId = Uuidv7Generator.NewUuid7().ToString();

            //Create folder for this assignment
            var folderPath = Path.Combine(_env.WebRootPath, "uploads", "Assignments", dto.ClassrooomID, assignId);
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
                    if (file == null || file.Length == 0)
                        continue;


                    var originalFileName = Path.GetFileName(file.FileName);
                    var uniquaName = $"{Uuidv7Generator.NewUuid7()}_{originalFileName}";
                    var fullPath = Path.Combine(folderPath, uniquaName);

                    using (var stream = new FileStream(fullPath, FileMode.Create))
                        await file.CopyToAsync(stream);


                    var relativePath = $"/uploads/Assignments/{dto.ClassrooomID}/{assignId}/{uniquaName}";

                    var assignmentFile = new AssignmentFile
                    {
                        FileId = Uuidv7Generator.NewUuid7().ToString(),
                        AssignId = assignId,
                        FileName = originalFileName,
                        FilePath = relativePath,
                        FileType = file.ContentType,
                    };
                    await _context.AssignmentFiles.AddAsync(assignmentFile);


                }
            }
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
            await _context.SaveChangesAsync();
            return assignId;
        }

        //==========Service to get assignment detail for Staff/Lecturer/Admin==========
        ///<summary>
        ///Lấy chi tiết bài tập cho giảng viên/giáo viên
        ///Bao gồm thông tin bài tập và các tệp đính kèm
        ///</summary>
        public async Task<StaffAssignmentDetailDto?> StaffGetAssignmentDetailAysnc(string assigmentId, string classroomId)
        {
            //Load assignment DB
            var assignmentData = await _context.Assignments
                .Where(a => a.AssignId == assigmentId && a.ClassroomId == classroomId)
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
                .Where(f => f.AssignId == assigmentId && f.Assign.ClassroomId == classroomId)
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
                .ToDictionaryAsync(s => s.StudentId, s => s.FirstName + " " + s.LastName);

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
                    Grade = sub?.Grade,
                    FeedBack = sub?.FeedBack

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

        //==========SERVICE TO GET STUDENT'S LATEST SUBMISSION DETAIL==========
        public async Task<GetSubmissionDetailDto> GetLatestSubmissionDetail(string assignemntId, string studentId)
        {
            //Take new assignment submit from student
            var latestSubmission = await _context.Submissions
                .Where(s => s.AssignId == assignemntId && s.StudentId == studentId)
                .OrderByDescending(s => s.SubmitAt)
                .FirstOrDefaultAsync();

            if (latestSubmission == null)
                return null;

            //Take Assigment infor
            var assignment = await _context.Assignments.FirstOrDefaultAsync(a => a.AssignId == assignemntId);
            var student = await _context.Students.FirstOrDefaultAsync(s => s.StudentId == studentId);

            if (assignment == null || student == null)
            {
                _logger.LogWarning("Assignment {AssignmentId} or Student {StudentId} not found when fetching submission detail.", assignemntId, studentId);
                return null;
            }

            //Take file from assignemt submit
            var file = await _context.SubmitFiles
                .Where(f => f.SubmitId == latestSubmission.SubmitId)
                .Select(f => new AssigmentFileDto
                {
                    FileName = f.FileName,
                    FilePath = f.FilePath,
                    FileType = f.FileType
                })
                .ToListAsync();

            //Map into DTO
            var resultDtos = new GetSubmissionDetailDto
            {
                StudentID = student.StudentId,
                FullName = student.LastName + " " + student.FirstName,
                AssignmentID = assignment.AssignId,
                AssginTitle = assignment.Title,
                Deadline = assignment.Deadline,
                SubmitAt = latestSubmission.SubmitAt,
                SubmitType = latestSubmission.SubmitType,
                Grade = latestSubmission.Grade,
                FeedBack = latestSubmission.FeedBack,
                SubmissionFile = file
            };
            return resultDtos;
        }

        public async Task<bool> GradeSubmissionAsync(string classroomId, string assignmentId, GradeSubmissionDto dto, string staffId)
        {
            //Check faculty privileges
            var isLecturerOfClass = await _context.ClassroomMembers
                .AnyAsync(cm => cm.ClassroomId == classroomId && cm.LecturerId == staffId && cm.RoleInClass != "Student");

            if (!isLecturerOfClass)
            {
                _logger.LogWarning("Staff {StaffId} is not authorized to grade in classroom {ClassroomId}", staffId, classroomId);
                return false;
            }

            //find Latest submit from student
            var submission = await _context.Submissions
                .Where(s => s.AssignId == assignmentId && s.StudentId == dto.StudentId)
                .OrderByDescending(s => s.SubmitAt)
                .FirstOrDefaultAsync();

            if (submission == null)
            {
                _logger.LogWarning("Submission for assignment {AssignmentId} and student {StudentId} not found.", assignmentId, dto.StudentId);
                return false;
            }

            // 3. Cập nhật điểm và phản hồi
            submission.Grade = dto.Grade;
            submission.FeedBack = dto.FeedBack?.Trim(); // Có thể null

            _context.Submissions.Update(submission);

            try
            {
                var log = new ActivityLog
                {
                    LogId = Uuidv7Generator.NewUuid7().ToString(),
                    StaffId = staffId,
                    Action = $"Grade Submission: {dto.Grade}",
                    TargetId = submission.SubmitId,
                    TargetTable = "Submissions",
                    TargetName = $"Assign: {assignmentId}, Student: {dto.StudentId}",
                    Timestap = DateTime.UtcNow
                };
                await _context.ActivityLogs.AddAsync(log);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error logging activity for grading submission {SubmissionId}", submission.SubmitId);
            }

            // 5. Lưu và trả về
            await _context.SaveChangesAsync();
            return true;
        }

        //==========SERVICE TO DOWNLOAND SUBMIT FILE==========
        public async Task<FileDownloadInfo?> GetSubmissionFileForTeacherAsync(string classroomId, string fileId)
        {
            //Find the file record in the SubmitFiles table.
            var fileRec = await _context.SubmitFiles
                .AsNoTracking()
                .Include(sf => sf.Submit)
                .ThenInclude(s => s.Assign)
                .FirstOrDefaultAsync(sf => sf.FileId == fileId && sf.Submit.Assign.ClassroomId == classroomId);

            if (fileRec == null || string.IsNullOrWhiteSpace(fileRec.FilePath))
                return null;

            //Convert to a physical path (Use the Path.Combine function similar to StudentAssignmentService)
            var relativePath = fileRec.FilePath.TrimStart('~').TrimStart('/');
            var physicalPath = Path.Combine(_env.WebRootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(physicalPath))
                return null;

            //Return the correct FileDownloadInfo object.
            return new FileDownloadInfo
            {
                PhysicalPath = physicalPath,
                ContentType = "application/octet-stream",
                DownloadName = fileRec.FileName ?? Path.GetFileName(physicalPath)
            };
        }

        //==========Service to update an existing assignment==========
        public async Task<bool> UpdateAssignmentAsync(string ClassroomId, string assignmentId, UpdateAssignmentDto dto, string staffId, string webRootPath)
        {
            //Load assignment and File
            var assigment = await _context.Assignments
                .Include(a => a.AssignmentFiles)
                .FirstOrDefaultAsync(a => a.AssignId == dto.AssignmentID && a.ClassroomId == ClassroomId);

            if (assigment == null)
                return false;

            //Begin transaction to keep consistency
            using var tx = await _context.Database.BeginTransactionAsync();

            try
            {
                //update basic fields
                if (!string.IsNullOrWhiteSpace(dto.Title))
                    assigment.Title = dto.Title.Trim();

                if (!string.IsNullOrWhiteSpace(dto.Description))
                    assigment.Description = dto.Description.Trim();

                if (dto.Deadline.HasValue)
                {
                    assigment.Deadline = dto.Deadline.Value;
                    assigment.DeadlineStatus = assigment.Deadline > DateTime.UtcNow ? "Valid" : "Overdue";
                }

                if (!string.IsNullOrWhiteSpace(dto.HomeworkStatus))
                    assigment.HomeworkStatus = dto.HomeworkStatus.Trim();

                var rootPath = Path.Combine(webRootPath, "uploads", "Assignments", assigment.ClassroomId, assigment.AssignId);
                Directory.CreateDirectory(rootPath);

                var fileId = Uuidv7Generator.NewUuid7().ToString();

                //===Add new file
                if (dto.NewFiles != null)
                {
                    foreach (var file in dto.NewFiles)
                    {
                        try
                        {
                            var originalName = Path.GetFileName(file.FileName);
                            var uniqueName = $"{fileId}_{originalName}";
                            var filePath = Path.Combine(rootPath, uniqueName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                                await file.CopyToAsync(stream);

                            _context.AssignmentFiles.Add(new AssignmentFile
                            {
                                FileId = fileId,
                                AssignId = assigment.AssignId,
                                FileName = originalName,
                                FilePath = $"/uploads/Assignments/{assigment.ClassroomId}/{assigment.AssignId}/{uniqueName}",
                                FileType = file.ContentType
                            });

                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error moving file for assignment {AssignmentId}", assignmentId);
                            // Continue moving other files
                        }
                    }
                }

                // 4. Remove requested files
                if (dto.RemoveFileId != null && dto.RemoveFileId.Any())
                {
                    var removeList = assigment.AssignmentFiles
                        .Where(f => dto.RemoveFileId.Contains(f.FileId))
                        .ToList();

                    foreach (var f in removeList)
                    {
                        try
                        {
                            var fullPath = Path.Combine(_env.WebRootPath, f.FilePath.TrimStart('/'));

                            if (File.Exists(fullPath))
                                File.Delete(fullPath);

                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "Error deleting physical file for assignment {AssignmentId}", assignmentId);
                        }

                        _context.AssignmentFiles.Remove(f);
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
        public async Task<bool> DeleteAssignmentAsync(string classroomId, string assignmentId, string staffId, string webRootPath)
        {

            //Load assignment with files
            var assignment = await _context.Assignments
                .Include(a => a.AssignmentFiles)
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId && a.ClassroomId == classroomId);

            if (assignment == null)
                return false;

            //Transaction -> ensuers no partial delete
            using var tx = await _context.Database.BeginTransactionAsync();

            //==Delete Folder
            var rootPath = Path.Combine(_env.WebRootPath, "uploads", "Assignments", assignment.ClassroomId, assignment.AssignId);

            if (Directory.Exists(rootPath))
                Directory.Delete(rootPath, true);

            var files = _context.AssignmentFiles.Where(f => f.AssignId == assignmentId).ToList();

            //===Delete DB records===
            _context.AssignmentFiles.RemoveRange(files);
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
    }
}
