using LmsMini.Application.DTOs.ClassAssignment;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/{classroomId}/Assignment")]
    public class AssignmentController : Controller
    {
        private readonly IAssigmentService _assigment;
        private readonly LmsDbContext _context;

        public AssignmentController (IAssigmentService assigment, LmsDbContext context)
        {
            _assigment = assigment;
            _context = context;
        }



        //Create Assignment with file uploads
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("create-assignment")]
        public async Task<IActionResult> CreateAssignment([FromForm] CreateAssigmentWithFilesDto dto)
        {
            var userId = User.Identity?.Name;
            

            var assignId = await _assigment.CreateAssignmentWithFilesAsync(dto, userId);

            return Ok(new { AssignmentID = assignId });
        }

        //Get Assignment Detail for Staff/Lecturer/Admin
        //Include assignment infor, attached files, submissions summary
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpGet("{assignmentId}/staff-assignment-detail")]
        public async Task<IActionResult> GetAssignmentDetail([FromRoute] string assignmentId, string classroomId)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return BadRequest("Assignment ID is required.");
            
            var assignmentDetail = await _assigment.StaffGetAssignmentDetailAysnc(assignmentId, classroomId);

            if (assignmentDetail == null)
                return NotFound("Assignment not found.");

            return Ok(assignmentDetail);
        }

        //===========CONTROLLER TO GET ASSIGNMENT SUBMIT FORM STUDENT==========
        [Authorize(Roles = "Staff, Lecturer, Admin")]
        [HttpGet("{assignmentId}/submission/{studentId}/detail")]
        public async Task<IActionResult> GetSubmissionDetail([FromRoute]string classroomId, [FromRoute]string assignmentId, [FromRoute]string studentId)
        {
            if (string.IsNullOrWhiteSpace(assignmentId) || string.IsNullOrWhiteSpace(studentId))
            {
                return BadRequest("Assignment ID and Student ID are required.");
            }

            var submissionDetail = await _assigment.GetLatestSubmissionDetail(assignmentId, studentId);

            if (submissionDetail == null)
            {
                return NotFound(new { Message = "Doesn't find assignment or doesn't find assignment" });
            }

            return Ok(submissionDetail);
        }

        //==========CONTROLLER TO RATING AND FEEDBACK========== 
        [Authorize(Roles ="Staff, Lecturer, Admin")]
        [HttpPut("{assignmentId}/grade-submission")]
        public async Task<IActionResult> GradeSubmission([FromRoute]string classroomId, [FromRoute]string assignmentId, [FromBody]GradeSubmissionDto dto)
        {
            var userId = User.Identity?.Name;

            if (dto.Grade < 0 || dto.Grade > 100)
            {
                return BadRequest("The score must be between 0 and 100");
            }

            var ok = await _assigment.GradeSubmissionAsync(classroomId, assignmentId, dto, userId);

            if (!ok)
            {
                return StatusCode(500, new
                {
                    Message = "Chấm điểm thất bại. Kiểm tra lại quyền hạn hoặc bài nộp/bài tập có tồn tại không."
                });
            }

            return NoContent(); //send 204 if not update success.
        }

        //========== CONTROLLER TO UPDATE EXISTING GRADE (CHẤM LẠI) ==========
        [Authorize(Roles = "Staff, Lecturer, Admin")]
        [HttpPatch("{assignmentId}/update-grade")] // URL: api/{classroomId}/Assignment/{assignmentId}/update-grade
        public async Task<IActionResult> ReGradeSubmission([FromRoute] string classroomId, [FromRoute] string assignmentId, [FromBody] GradeSubmissionDto dto)
        {
            // 1. Lấy StaffId từ Identity Name (Username) như các hàm chấm điểm trước của bạn
            var userId = User.Identity?.Name;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 2. Validate thang điểm (Thang 100 theo logic mới của bạn)
            if (dto.Grade < 0 || dto.Grade > 100)
            {
                return BadRequest("The score must be between 0 and 100");
            }

            // 3. Gọi service xử lý cập nhật
            var success = await _assigment.UpdateExistingGradeAsync(classroomId, assignmentId, dto, userId);

            if (!success)
            {
                return StatusCode(400, new
                {
                    Message = "Cập nhật điểm thất bại. Vui lòng kiểm tra bài nộp đã có điểm trước đó chưa hoặc quyền hạn của bạn."
                });
            }

            return NoContent(); // 204 Success
        }

        [Authorize(Roles = "Staff, Lecturer, Admin")]
        [HttpGet("{assignmentId}/submissions/download-file/{fileId}")]
        public async Task<IActionResult> DownloadSubmissionFile(string classroomId, string fileId)
        {
            var fileInfo = await _assigment.GetSubmissionFileForTeacherAsync(classroomId, fileId);

            if (fileInfo == null) return NotFound("File not found.");

            // Đảm bảo ContentType là loại file tải về nếu bạn muốn ép Chrome không mở trực tiếp
            // "application/octet-stream" là loại nhị phân chung buộc trình duyệt phải tải
            var contentType = "application/octet-stream";

            // Thêm header để báo trình duyệt đây là file đính kèm (attachment)
            Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileInfo.DownloadName}\"");

            return PhysicalFile(fileInfo.PhysicalPath, contentType, fileInfo.DownloadName);
        }

        //===========UPDATE ASSIGNMENT ==============
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPut("update-assignment/{assignmentId}")]
        //Update Assignment
        public async Task<IActionResult> UpdateAssignment(string classroomId,[FromRoute] string assignmentId, [FromForm] UpdateAssignmentDto dto)
        {
            dto.ClasroomID = classroomId;
            dto.AssignmentID = assignmentId;
            var userId = User.Identity?.Name;

            var ok = await _assigment.UpdateAssignmentAsync(classroomId, assignmentId, dto, userId);
            if (!ok)
                return StatusCode(500, "Failed to update assignment.");

            return NoContent();
        }

        //===========DELETE ASSIGNMENT ==============
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpDelete("delete-assignment/{assignmentId}")]
        public async Task<IActionResult> DeleteAssignment(string classroomId, string assignmentId)
        {
            //StaffId stored in JWT claim
            var userId = User.Identity?.Name;

            

            var ok = await _assigment.DeleteAssignmentAsync(classroomId, assignmentId, userId);

            if (!ok)
                return NotFound("Assignment not found or unanble to delete");

            return NoContent();
        }
    }
}
