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
        private readonly IWebHostEnvironment _env;
        private readonly LmsDbContext _context;

        public AssignmentController (IAssigmentService assigment, IWebHostEnvironment env, LmsDbContext context)
        {
            _assigment = assigment;
            _env = env;
            _context = context;
        }



        //Create Assignment with file uploads
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("create-assignment")]
        public async Task<IActionResult> CreateAssignment([FromForm] CreateAssigmentWithFilesDto dto)
        {
            var staffId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == staffId);

            if (staff == null)
            {

                return StatusCode(403, new
                {
                    Message = "Không thể xác định nhân viên thực hiện tác vụ này trong hệ thống nghiệp vụ."
                });
            }

            var assignId = await _assigment.CreateAssignmentWithFilesAsync(dto, staff.StaffId, _env.WebRootPath);

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

        //===========UPDATE ASSIGNMENT ==============
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPut("update-assignment/{assignmentId}")]
        //Update Assignment
        public async Task<IActionResult> UpdateAssignment(string classroomId,[FromRoute] string assignmentId, [FromForm] UpdateAssignmentDto dto)
        {
            dto.ClasroomID = classroomId;
            dto.AssignmentID = assignmentId;
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff == null)
            {

                return StatusCode(403, new
                {
                    Message = "Không thể xác định nhân viên thực hiện tác vụ này trong hệ thống nghiệp vụ."
                });
            }

            var webRootPath = _env.WebRootPath;
            if(string.IsNullOrEmpty(webRootPath))
                return StatusCode(500, "Web root path is not configured.");

            var ok = await _assigment.UpdateAssignmentAsync(classroomId, assignmentId, dto, staff.StaffId, webRootPath);
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
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff == null)
            {

                return StatusCode(403, new
                {
                    Message = "Không thể xác định nhân viên thực hiện tác vụ này trong hệ thống nghiệp vụ."
                });
            }

            var ok = await _assigment.DeleteAssignmentAsync(classroomId, assignmentId, staff.StaffId, _env.WebRootPath);

            if (!ok)
                return NotFound("Assignment not found or unanble to delete");

            return NoContent();
        }
    }
}
