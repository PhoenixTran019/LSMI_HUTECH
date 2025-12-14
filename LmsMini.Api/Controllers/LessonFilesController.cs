using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Classroom/{classroomId}/Lessons/files")]
    public class LessonFilesController : ControllerBase
    {
        private readonly LmsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public LessonFilesController(LmsDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        /// <summary>
        /// Download file của Lesson theo FilesId
        /// Roles: Student / Staff / Lecturer / Admin
        /// </summary>
        [Authorize(Roles = "Student,Staff,Lecturer,Admin")]
        [HttpGet("{filesId}/download")]
        public async Task<IActionResult> DownloadLessonFile(string classroomId, string filesId)
        {
            // 1) Lấy userId từ token (sub -> thường map về NameIdentifier)
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("Cannot identify user from token.");

            // 2) Lấy record file + verify file thuộc lesson thuộc classroomId
            var fileRec = await _context.LessonFiles
                .AsNoTracking()
                .Include(f => f.Lesson)
                .FirstOrDefaultAsync(f =>
                    f.FilesId == filesId
                    && f.Lesson != null
                    && f.Lesson.ClassroomId == classroomId);

            if (fileRec == null)
                return NotFound("Lesson file not found.");

            // 3) Nếu là Student thì check membership lớp
            //    (Student token có userId là UUID -> map qua Student.UserId để lấy StudentId)
            if (User.IsInRole("Student"))
            {
                var studentId = await _context.Students
                    .Where(s => s.UserId == userId)
                    .Select(s => s.StudentId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(studentId))
                    return Unauthorized("Student profile not found for this token.");

                var isMember = await _context.ClassroomMembers
                    .AnyAsync(m => m.ClassroomId == classroomId && m.StudentId == studentId);

                if (!isMember)
                    return Forbid("You are not a member of this classroom.");
            }

            // 4) Build physical path từ FilePath trong DB
            if (string.IsNullOrWhiteSpace(fileRec.FilePath))
                return StatusCode(500, "FilePath is empty in database.");

            var webRoot = _env.WebRootPath;
            if (string.IsNullOrWhiteSpace(webRoot))
                return StatusCode(500, "WebRootPath is not configured.");

            // FilePath DB dạng: /uploads/Lessons/{classroomId}/{lessonId}/{fileName}
            var relative = fileRec.FilePath.TrimStart('/')
                .Replace('/', Path.DirectorySeparatorChar);

            var physicalPath = Path.Combine(webRoot, relative);

            if (!System.IO.File.Exists(physicalPath))
                return NotFound("Physical file not found on server.");

            // 5) Stream file
            var contentType = string.IsNullOrWhiteSpace(fileRec.FileType)
                ? "application/octet-stream"
                : fileRec.FileType;

            var downloadName = string.IsNullOrWhiteSpace(fileRec.FileName)
                ? Path.GetFileName(physicalPath)
                : fileRec.FileName;

            return PhysicalFile(physicalPath, contentType, downloadName, enableRangeProcessing: true);
        }
    }
}
