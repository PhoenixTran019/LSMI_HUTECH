using LmsMini.Application.Common.Helpers;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Classroom/{classroomId}/Student/Assignments")]
    [Authorize(Roles = "Student")]
    public class StudentAssignmentController : ControllerBase
    {
        private readonly LmsDbContext _context;
        private readonly IStudentAssignmentService _assignmentService;

        public StudentAssignmentController(LmsDbContext context, IStudentAssignmentService assignmentService)
        {
            _assignmentService = assignmentService;
            _context = context;
        }

        private string? GetUserId() => User.Identity?.Name;

        // =========================
        // Helpers
        // =========================
       

        // =========================
        // 1) Student list assignments
        // =========================
        [HttpGet]
        public async Task<IActionResult> GetAssignments(string classroomId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _assignmentService.GetAssignmentsAsync(classroomId, userId);
            return Ok(result);
        }

        // =========================
        // 2) Student assignment detail (Description + files + latest submission)
        // =========================
        [HttpGet("{assignmentId}/detail")]
        public async Task<IActionResult> GetAssignmentDetail(string classroomId, string assignmentId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var detail = await _assignmentService.GetAssignmentDetailAsync(classroomId, assignmentId, userId);
            if (detail == null) return NotFound("Assignment not found.");

            return Ok(detail);
        }

        // =========================
        // 3) Download assignment attached file (Student)
        // =========================
        [HttpGet("files/{fileId}/download")]
        public async Task<IActionResult> DownloadAssignmentFile(string classroomId, string fileId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Service đã xử lý ToPhysical() và check file tồn tại
            var info = await _assignmentService.GetAssignmentFileForDownloadAsync(classroomId, fileId, userId);
            if (info == null) return NotFound("File not found on server.");

            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{Uri.EscapeDataString(info.DownloadName)}\"");
            return PhysicalFile(info.PhysicalPath, info.ContentType, info.DownloadName);
        }

        // =========================
        // 4) Student submit assignment (multipart/form-data)
        // =========================
        [HttpPost("{assignmentId}/submit")]
        [RequestSizeLimit(1_000_000_000)]
        public async Task<IActionResult> SubmitAssignment(
            string classroomId,
            string assignmentId,
            [FromForm] StudentSubmitAssignmentRequest request)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var result = await _assignmentService.SubmitAssignmentAsync(
                    classroomId,
                    assignmentId,
                    userId,
                    request.SubmitType,
                    request.Files);

                return Ok(result);
            }
            catch (Exception ex)
            {
                // Các lỗi như: File trống, không phải thành viên... được ném ra từ Service
                return BadRequest(new { message = ex.Message });
            }
        }

        // =========================
        // 5) Student download their submitted file
        // =========================
        [HttpGet("submission-files/{submitFileId}/download")]
        public async Task<IActionResult> DownloadMySubmissionFile(string classroomId, string submitFileId)
        {
            var userId = GetUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var info = await _assignmentService.GetMySubmissionFileForDownloadAsync(classroomId, submitFileId, userId);
            if (info == null) return NotFound("Submission file not found.");

            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{Uri.EscapeDataString(info.DownloadName)}\"");
            return PhysicalFile(info.PhysicalPath, info.ContentType, info.DownloadName);
        }
    }

    public class StudentSubmitAssignmentRequest
    {
        public string? SubmitType { get; set; }

        // FE form-data key phải là "Files"
        [FromForm(Name = "Files")]
        public List<IFormFile> Files { get; set; } = new();
    }
}