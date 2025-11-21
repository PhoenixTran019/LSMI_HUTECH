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
    }
}
