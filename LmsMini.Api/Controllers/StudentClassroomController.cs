using LmsMini.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Student/Classroom")]
    [Authorize(Roles = "Student")]
    public class StudentClassroomController : Controller
    {
        private readonly IStudentClassroomService _studentClassroomService;

        public StudentClassroomController (IStudentClassroomService studentClassroomService)
        {
            _studentClassroomService = studentClassroomService;
        }

        //==========CONTROLLER TO STUDENT JOIN CLASSROOM==========
        [HttpPost("JoinClass-bycode")]
        public async Task<IActionResult> JoinClassroomByCode([FromBody] string inviteCode)
        {
            //Take ID from Token
            var studentId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(studentId))
                return Unauthorized("Student identity cannot be determined");

            if (String.IsNullOrEmpty(inviteCode))
                return BadRequest("Invite code is required");

            var classroomId = await _studentClassroomService.JoinClassroomByCodeAsync(inviteCode, studentId);

            if (classroomId == null)
            {
                return NotFound("Classroom not found or invite code is invalid/inactive.");
            }

            return Ok(new { ClassroomId = classroomId, Message = "Successfully joined the classroom." });
        }
    }
}
