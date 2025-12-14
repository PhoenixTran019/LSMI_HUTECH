using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.Common;
using LmsMini.Application.DTOs.Student;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace LmsMini.Infrastructure.Services.Students
{
    public class StudentAssignmentService : IStudentAssignmentService
    {
        private readonly LmsDbContext _context;

        public StudentAssignmentService(LmsDbContext context)
        {
            _context = context;
        }

        private async Task<string> GetStudentIdOrThrow(string userId)
        {
            var studentId = await _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(studentId))
                throw new InvalidOperationException("Student profile not found for this token.");

            return studentId;
        }

        private async Task EnsureMemberOrThrow(string classroomId, string studentId)
        {
            var isMember = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId && m.StudentId == studentId);

            if (!isMember)
                throw new UnauthorizedAccessException("You are not a member of this classroom.");
        }

        private static string ToPhysical(string webRootPath, string filePath)
        {
            return Path.Combine(webRootPath, filePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
        }

        public async Task<List<StudentAssignmentListItemDto>> GetAssignmentsAsync(string classroomId, string userId)
        {
            var studentId = await GetStudentIdOrThrow(userId);
            await EnsureMemberOrThrow(classroomId, studentId);

            var assignments = await _context.Assignments
                .AsNoTracking()
                .Where(a => a.ClassroomId == classroomId)
                .Select(a => new
                {
                    a.AssignId,
                    a.Title,
                    a.Description,
                    a.Deadline,
                    a.DeadlineStatus,
                    a.HomeworkStatus,
                    a.CreateAt,
                    FileCount = a.AssignmentFiles.Count
                })
                .OrderByDescending(x => x.CreateAt)
                .ToListAsync();

            var assignIds = assignments.Where(x => x.AssignId != null).Select(x => x.AssignId!).ToList();

            var latestSubs = await _context.Submissions
                .AsNoTracking()
                .Where(s => s.StudentId == studentId && s.AssignId != null && assignIds.Contains(s.AssignId))
                .GroupBy(s => s.AssignId!)
                .Select(g => g.OrderByDescending(x => x.SubmitAt).FirstOrDefault())
                .ToListAsync();

            var subMap = latestSubs
                .Where(x => x != null && x.AssignId != null)
                .ToDictionary(x => x!.AssignId!, x => x!);

            return assignments.Select(a =>
            {
                subMap.TryGetValue(a.AssignId!, out var sub);
                var submittedAt = sub?.SubmitAt;
                var isLate = submittedAt.HasValue && a.Deadline.HasValue && submittedAt.Value > a.Deadline.Value;

                return new StudentAssignmentListItemDto
                {
                    AssignId = a.AssignId,
                    Title = a.Title,
                    Description = a.Description,
                    Deadline = a.Deadline,
                    DeadlineStatus = a.DeadlineStatus,
                    HomeworkStatus = a.HomeworkStatus,
                    CreateAt = a.CreateAt,
                    FileCount = a.FileCount,
                    HasSubmitted = sub != null,
                    SubmittedAt = submittedAt,
                    IsLate = isLate,
                    SubmitType = sub?.SubmitType
                };
            }).ToList();
        }

        public async Task<StudentAssignmentDetailDto?> GetAssignmentDetailAsync(string classroomId, string assignmentId, string userId)
        {
            var studentId = await GetStudentIdOrThrow(userId);
            await EnsureMemberOrThrow(classroomId, studentId);

            var assignment = await _context.Assignments
                .AsNoTracking()
                .Include(a => a.AssignmentFiles)
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId && a.ClassroomId == classroomId);

            if (assignment == null) return null;

            var latest = await _context.Submissions
                .AsNoTracking()
                .Include(s => s.SubmitFiles)
                .Where(s => s.AssignId == assignmentId && s.StudentId == studentId)
                .OrderByDescending(s => s.SubmitAt)
                .FirstOrDefaultAsync();

            return new StudentAssignmentDetailDto
            {
                AssignId = assignment.AssignId,
                Title = assignment.Title,
                Description = assignment.Description,
                Deadline = assignment.Deadline,
                DeadlineStatus = assignment.DeadlineStatus,
                HomeworkStatus = assignment.HomeworkStatus,
                CreateAt = assignment.CreateAt,
                Files = assignment.AssignmentFiles.Select(f => new StudentAssignmentFileDto
                {
                    FileId = f.FileId,
                    FileName = f.FileName,
                    FileType = f.FileType
                }).ToList(),
                LatestSubmission = latest == null ? null : new StudentLatestSubmissionDto
                {
                    SubmitId = latest.SubmitId,
                    SubmitAt = latest.SubmitAt,
                    SubmitType = latest.SubmitType,
                    FeedBack = latest.FeedBack,
                    Grade = latest.Grade.HasValue ? (decimal?)Convert.ToDecimal(latest.Grade.Value) : null,

                    Files = latest.SubmitFiles.Select(sf => new StudentSubmitFileDto
                    {
                        FileId = sf.FileId,
                        FileName = sf.FileName,
                        FileType = sf.FileType
                    }).ToList()
                }
            };
        }

        public async Task<FileDownloadInfo?> GetAssignmentFileForDownloadAsync(string classroomId, string fileId, string userId, string webRootPath)
        {
            var studentId = await GetStudentIdOrThrow(userId);
            await EnsureMemberOrThrow(classroomId, studentId);

            var fileRec = await _context.AssignmentFiles
                .AsNoTracking()
                .Include(f => f.Assign)
                .FirstOrDefaultAsync(f =>
                    f.FileId == fileId
                    && f.Assign != null
                    && f.Assign.ClassroomId == classroomId);

            if (fileRec == null) return null;
            if (string.IsNullOrWhiteSpace(fileRec.FilePath)) return null;

            var physical = ToPhysical(webRootPath, fileRec.FilePath);
            if (!File.Exists(physical)) return null;

            return new FileDownloadInfo
            {
                PhysicalPath = physical,
                ContentType = string.IsNullOrWhiteSpace(fileRec.FileType) ? "application/octet-stream" : fileRec.FileType!,
                DownloadName = string.IsNullOrWhiteSpace(fileRec.FileName) ? Path.GetFileName(physical) : fileRec.FileName!
            };
        }

        public async Task<StudentSubmitResultDto> SubmitAssignmentAsync(
            string classroomId,
            string assignmentId,
            string userId,
            string? submitType,
            List<IFormFile> files,
            string webRootPath)
        {
            var studentId = await GetStudentIdOrThrow(userId);
            await EnsureMemberOrThrow(classroomId, studentId);

            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId && a.ClassroomId == classroomId);

            if (assignment == null)
                throw new KeyNotFoundException("Assignment not found.");

            if (files == null || files.Count == 0)
                throw new InvalidOperationException("No files provided.");

            var now = DateTime.UtcNow;
            var finalType = !string.IsNullOrWhiteSpace(submitType)
                ? submitType.Trim()
                : (assignment.Deadline.HasValue && now > assignment.Deadline.Value ? "Late" : "OnTime");

            var submitId = Uuidv7Generator.NewUuid7().ToString();

            var submission = new Submission
            {
                SubmitId = submitId,
                AssignId = assignmentId,
                StudentId = studentId,
                SubmitAt = now,
                SubmitType = finalType,
                FeedBack = null,
                Grade = null
            };

            await _context.Submissions.AddAsync(submission);

            var folder = Path.Combine(webRootPath, "uploads", "Submissions", classroomId, assignmentId, studentId, submitId);
            Directory.CreateDirectory(folder);

            foreach (var f in files)
            {
                if (f == null || f.Length == 0) continue;

                var fileId = Uuidv7Generator.NewUuid7().ToString();
                var original = Path.GetFileName(f.FileName);
                var uniqueName = $"{fileId}_{original}";
                var fullPath = Path.Combine(folder, uniqueName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await f.CopyToAsync(stream);
                }

                var relative = $"/uploads/Submissions/{classroomId}/{assignmentId}/{studentId}/{submitId}/{uniqueName}";

                var submitFile = new SubmitFile
                {
                    FileId = fileId,
                    SubmitId = submitId,
                    FileName = original,
                    FilePath = relative,
                    FileType = f.ContentType,
                    UpdateAt = now
                };

                await _context.SubmitFiles.AddAsync(submitFile);
            }

            await _context.SaveChangesAsync();

            return new StudentSubmitResultDto
            {
                SubmitId = submitId,
                AssignId = assignmentId,
                StudentId = studentId,
                SubmitAt = now,
                SubmitType = finalType
            };
        }

        public async Task<FileDownloadInfo?> GetMySubmissionFileForDownloadAsync(string classroomId, string submitFileId, string userId, string webRootPath)
        {
            var studentId = await GetStudentIdOrThrow(userId);
            await EnsureMemberOrThrow(classroomId, studentId);

            var fileRec = await _context.SubmitFiles
                .AsNoTracking()
                .Include(sf => sf.Submit)
                .ThenInclude(s => s!.Assign)
                .FirstOrDefaultAsync(sf =>
                    sf.FileId == submitFileId
                    && sf.Submit != null
                    && sf.Submit.StudentId == studentId
                    && sf.Submit.Assign != null
                    && sf.Submit.Assign.ClassroomId == classroomId);

            if (fileRec == null) return null;
            if (string.IsNullOrWhiteSpace(fileRec.FilePath)) return null;

            var physical = ToPhysical(webRootPath, fileRec.FilePath);
            if (!File.Exists(physical)) return null;

            return new FileDownloadInfo
            {
                PhysicalPath = physical,
                ContentType = string.IsNullOrWhiteSpace(fileRec.FileType) ? "application/octet-stream" : fileRec.FileType!,
                DownloadName = string.IsNullOrWhiteSpace(fileRec.FileName) ? Path.GetFileName(physical) : fileRec.FileName!
            };
        }
    }
}
