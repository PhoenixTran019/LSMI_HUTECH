using LmsMini.Application.DTOs.ClassAssignment;
using LmsMini.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/{classroomId}/Lessons")]
    public class AssignmentController : Controller
    {
        private readonly IAssigmentService _assigment;
        private readonly IWebHostEnvironment _env;

        public AssignmentController(IAssigmentService assigment, IWebHostEnvironment env)
        {
            _assigment = assigment;
            _env = env;
        }

        //Create Assignment with file uploads
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("create-assignment")]
        public async Task<IActionResult> CreateAssignment([FromForm] CreateAssigmentWithFilesDto dto)
        {
            var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(teacherId))
                return Unauthorized("Cannot identify teacher from token.");

            var assignId = await _assigment.CreateAssignmentWithFilesAsync(dto, teacherId, _env.WebRootPath);

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


        //===========UPDATE ASSIGNMENT ==============
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPut("update-assignment/{assignmentId}")]
        //Update Assignment
        public async Task<IActionResult> UpdateAssignment(string classroomId,[FromRoute] string assignmentId, [FromForm] UpdateAssignmentDto dto)
        {
            dto.ClasroomID = classroomId;
            dto.AssignmentID = assignmentId;
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffId))
                return Unauthorized("Cannot identify staff from token.");

            var webRootPath = _env.WebRootPath;
            if(string.IsNullOrEmpty(webRootPath))
                return StatusCode(500, "Web root path is not configured.");

            var ok = await _assigment.UpdateAssignmentAsync(classroomId, assignmentId, dto, staffId, webRootPath);
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
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(staffId))
                return Unauthorized("Cannot identify staff from token.");

            var ok = await _assigment.DeleteAssignmentAsync(classroomId, assignmentId, staffId, _env.WebRootPath);

            if (!ok)
                return NotFound("Assignment not found or unanble to delete");

            return NoContent();
        }
    }
}
