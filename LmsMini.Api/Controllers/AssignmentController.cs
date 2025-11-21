using LmsMini.Application.DTOs.ClassAssignment;
using LmsMini.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AssignmentController : Controller
    {
        private readonly IAssigmentService _assigment;
        private readonly IWebHostEnvironment _env;

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
        [HttpGet("staff-assignment-detail/{assignmentId}")]
        public async Task<IActionResult> GetAssignmentDetail([FromRoute] string assignmentId)
        {
            if (string.IsNullOrWhiteSpace(assignmentId))
                return BadRequest("Assignment ID is required.");

            var assignmentDetail = await _assigment.StaffGetAssignmentDetailAysnc(assignmentId);

            if (assignmentDetail == null)
                return NotFound("Assignment not found.");

            return Ok(assignmentDetail);
        }

        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPut("update-assignment/{assignmentId}")]
        //Update Assignment
        public async Task<IActionResult> UpdateAssignment([FromRoute] string assignmentId, [FromForm] UpdateAssignmentDto dto)
        {
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffId))
                return Unauthorized("Cannot identify staff from token.");

            var webRootPath = _env.WebRootPath;
            if(string.IsNullOrEmpty(webRootPath))
                return StatusCode(500, "Web root path is not configured.");

            var ok = await _assigment.UpdateAssignmentAsync(assignmentId, dto, staffId, webRootPath);
            if (!ok)
                return StatusCode(500, "Failed to update assignment.");

            return NoContent();
        }
    }
}
