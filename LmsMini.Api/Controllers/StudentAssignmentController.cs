using LmsMini.Application.Common.Helpers;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Classroom/{classroomId}/Student/Assignments")]
    public class StudentAssignmentController : ControllerBase
    {
        private readonly LmsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public StudentAssignmentController(LmsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        // =========================
        // Helpers
        // =========================
        private async Task<(string studentId, IActionResult? error)> GetStudentIdOrError()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return (string.Empty, Unauthorized("Cannot identify user from token."));

            var studentId = await _context.Students
                .Where(s => s.UserId == userId)
                .Select(s => s.StudentId)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(studentId))
                return (string.Empty, Unauthorized("Student profile not found for this token."));

            return (studentId, null);
        }

        private async Task<IActionResult?> EnsureStudentIsMember(string classroomId, string studentId)
        {
            var isMember = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId
                            && m.StudentId == studentId
                            && (m.RoleInClass == null || m.RoleInClass == "Student"));

            if (!isMember)
                return Forbid("You are not a member of this classroom.");

            return null;
        }

        private string BuildPhysicalPath(string relativeFilePath)
        {
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                throw new InvalidOperationException("WebRootPath is not configured.");

            var relative = relativeFilePath.TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            return Path.Combine(webRoot, relative);
        }

        // =========================
        // 1) Student list assignments (kèm trạng thái đã nộp/chưa)
        // =========================
        [Authorize(Roles = "Student")]
        [HttpGet]
        public async Task<IActionResult> GetAssignments(string classroomId)
        {
            var (studentId, err) = await GetStudentIdOrError();
            if (err != null) return err;

            var memberErr = await EnsureStudentIsMember(classroomId, studentId);
            if (memberErr != null) return memberErr;

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
                .OrderByDescending(a => a.CreateAt)
                .ToListAsync();

            var assignIds = assignments.Select(a => a.AssignId).ToList();

            var latestSubmissions = await _context.Submissions
                .AsNoTracking()
                .Where(s => s.StudentId == studentId && s.AssignId != null && assignIds.Contains(s.AssignId))
                .GroupBy(s => s.AssignId!)
                .Select(g => g.OrderByDescending(x => x.SubmitAt).FirstOrDefault())
                .ToDictionaryAsync(x => x!.AssignId!, x => x);

            var result = assignments.Select(a =>
            {
                latestSubmissions.TryGetValue(a.AssignId!, out var sub);
                var submittedAt = sub?.SubmitAt;

                bool isLate = false;
                if (submittedAt.HasValue && a.Deadline.HasValue)
                    isLate = submittedAt.Value > a.Deadline.Value;

                return new
                {
                    a.AssignId,
                    a.Title,
                    a.Description,
                    a.Deadline,
                    a.DeadlineStatus,
                    a.HomeworkStatus,
                    a.CreateAt,
                    a.FileCount,
                    HasSubmitted = sub != null,
                    SubmittedAt = submittedAt,
                    IsLate = isLate,
                    SubmitType = sub?.SubmitType
                };
            });

            return Ok(result);
        }

        // =========================
        // 2) Student assignment detail (kèm file đính kèm + latest submission)
        // =========================
        [Authorize(Roles = "Student")]
        [HttpGet("{assignmentId}/detail")]
        public async Task<IActionResult> GetAssignmentDetail(string classroomId, string assignmentId)
        {
            var (studentId, err) = await GetStudentIdOrError();
            if (err != null) return err;

            var memberErr = await EnsureStudentIsMember(classroomId, studentId);
            if (memberErr != null) return memberErr;

            var assignment = await _context.Assignments
                .AsNoTracking()
                .Include(a => a.AssignmentFiles)
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId && a.ClassroomId == classroomId);

            if (assignment == null)
                return NotFound("Assignment not found.");

            var latestSubmission = await _context.Submissions
                .AsNoTracking()
                .Include(s => s.SubmitFiles)
                .Where(s => s.AssignId == assignmentId && s.StudentId == studentId)
                .OrderByDescending(s => s.SubmitAt)
                .FirstOrDefaultAsync();

            var response = new
            {
                assignment.AssignId,
                assignment.Title,
                assignment.Description,
                assignment.Deadline,
                assignment.DeadlineStatus,
                assignment.HomeworkStatus,
                assignment.CreateAt,
                Files = assignment.AssignmentFiles.Select(f => new
                {
                    f.FileId,
                    f.FileName,
                    f.FileType
                }).ToList(),
                LatestSubmission = latestSubmission == null ? null : new
                {
                    latestSubmission.SubmitId,
                    latestSubmission.SubmitAt,
                    latestSubmission.SubmitType,
                    latestSubmission.FeedBack,
                    latestSubmission.Grade,
                    Files = latestSubmission.SubmitFiles.Select(sf => new
                    {
                        sf.FileId,
                        sf.FileName,
                        sf.FileType
                    }).ToList()
                }
            };

            return Ok(response);
        }

        // =========================
        // 3) Download assignment attached file (Student)
        // =========================
        [Authorize(Roles = "Student")]
        [HttpGet("files/{fileId}/download")]
        public async Task<IActionResult> DownloadAssignmentFile(string classroomId, string fileId)
        {
            var (studentId, err) = await GetStudentIdOrError();
            if (err != null) return err;

            var memberErr = await EnsureStudentIsMember(classroomId, studentId);
            if (memberErr != null) return memberErr;

            var fileRec = await _context.AssignmentFiles
                .AsNoTracking()
                .Include(f => f.Assign)
                .FirstOrDefaultAsync(f =>
                    f.FileId == fileId
                    && f.Assign != null
                    && f.Assign.ClassroomId == classroomId);

            if (fileRec == null)
                return NotFound("Assignment file not found.");

            if (string.IsNullOrWhiteSpace(fileRec.FilePath))
                return StatusCode(500, "FilePath is empty in database.");

            var physicalPath = BuildPhysicalPath(fileRec.FilePath);

            if (!System.IO.File.Exists(physicalPath))
                return NotFound("Physical file not found on server.");

            var contentType = string.IsNullOrWhiteSpace(fileRec.FileType)
                ? "application/octet-stream"
                : fileRec.FileType;

            var downloadName = string.IsNullOrWhiteSpace(fileRec.FileName)
                ? Path.GetFileName(physicalPath)
                : fileRec.FileName;

            return PhysicalFile(physicalPath, contentType, downloadName, enableRangeProcessing: true);
        }

        // =========================
        // 4) Student submit assignment (multipart/form-data)
        // =========================
        // form-data:
        // - Files: (multiple files)
        // - SubmitType: (optional) "OnTime" / "Late" (nếu bỏ trống -> BE tự tính theo Deadline)
        [Authorize(Roles = "Student")]
        [HttpPost("{assignmentId}/submit")]
        [RequestSizeLimit(200_000_000)] // 200MB (bạn có thể đổi)
        public async Task<IActionResult> SubmitAssignment(
            string classroomId,
            string assignmentId,
            [FromForm] StudentSubmitAssignmentRequest request)
        {
            var (studentId, err) = await GetStudentIdOrError();
            if (err != null) return err;

            var memberErr = await EnsureStudentIsMember(classroomId, studentId);
            if (memberErr != null) return memberErr;

            var assignment = await _context.Assignments
                .FirstOrDefaultAsync(a => a.AssignId == assignmentId && a.ClassroomId == classroomId);

            if (assignment == null)
                return NotFound("Assignment not found.");

            if (request.Files == null || request.Files.Count == 0)
                return BadRequest("No files provided.");

            var submitId = Uuidv7Generator.NewUuid7().ToString();
            var now = DateTime.UtcNow;

            // Nếu FE không gửi SubmitType thì BE tự tính
            string submitType;
            if (!string.IsNullOrWhiteSpace(request.SubmitType))
            {
                submitType = request.SubmitType.Trim();
            }
            else
            {
                if (assignment.Deadline.HasValue && now > assignment.Deadline.Value)
                    submitType = "Late";
                else
                    submitType = "OnTime";
            }

            // Tạo Submission record
            var submission = new Submission
            {
                SubmitId = submitId,
                AssignId = assignmentId,
                StudentId = studentId,
                SubmitAt = now,
                SubmitType = submitType,
                FeedBack = null,
                Grade = null
            };

            await _context.Submissions.AddAsync(submission);

            // Lưu file vào wwwroot/uploads/Submissions/...
            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                return StatusCode(500, "WebRootPath is not configured.");

            var folderPath = Path.Combine(webRoot, "uploads", "Submissions", classroomId, assignmentId, studentId, submitId);
            Directory.CreateDirectory(folderPath);

            foreach (var file in request.Files)
            {
                if (file == null || file.Length == 0) continue;

                var fileId = Uuidv7Generator.NewUuid7().ToString();
                var original = Path.GetFileName(file.FileName);
                var uniqueName = $"{fileId}_{original}";
                var fullPath = Path.Combine(folderPath, uniqueName);

                using (var stream = new FileStream(fullPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                var relativePath = $"/uploads/Submissions/{classroomId}/{assignmentId}/{studentId}/{submitId}/{uniqueName}";

                var submitFile = new SubmitFile
                {
                    FileId = fileId,
                    SubmitId = submitId,
                    FileName = original,
                    FilePath = relativePath,
                    FileType = file.ContentType,
                    UpdateAt = now
                };

                await _context.SubmitFiles.AddAsync(submitFile);
            }

            await _context.SaveChangesAsync();

            return Ok(new
            {
                SubmitID = submitId,
                AssignID = assignmentId,
                StudentID = studentId,
                SubmitAt = now,
                SubmitType = submitType,
                Message = "Submit success"
            });
        }

        // =========================
        // 5) Student download their submitted file
        // =========================
        [Authorize(Roles = "Student")]
        [HttpGet("submission-files/{submitFileId}/download")]
        public async Task<IActionResult> DownloadMySubmissionFile(string classroomId, string submitFileId)
        {
            var (studentId, err) = await GetStudentIdOrError();
            if (err != null) return err;

            var memberErr = await EnsureStudentIsMember(classroomId, studentId);
            if (memberErr != null) return memberErr;

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

            if (fileRec == null)
                return NotFound("Submission file not found.");

            if (string.IsNullOrWhiteSpace(fileRec.FilePath))
                return StatusCode(500, "FilePath is empty in database.");

            var physicalPath = BuildPhysicalPath(fileRec.FilePath);

            if (!System.IO.File.Exists(physicalPath))
                return NotFound("Physical file not found on server.");

            var contentType = string.IsNullOrWhiteSpace(fileRec.FileType)
                ? "application/octet-stream"
                : fileRec.FileType;

            var downloadName = string.IsNullOrWhiteSpace(fileRec.FileName)
                ? Path.GetFileName(physicalPath)
                : fileRec.FileName;

            return PhysicalFile(physicalPath, contentType, downloadName, enableRangeProcessing: true);
        }
    }

    // =========================
    // Request model for multipart/form-data
    // =========================
    public class StudentSubmitAssignmentRequest
    {
        public string? SubmitType { get; set; } // optional: "OnTime" / "Late"
        public List<IFormFile> Files { get; set; } = new();
    }
}
