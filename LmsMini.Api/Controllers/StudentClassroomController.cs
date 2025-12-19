using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
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
        private readonly LmsDbContext _context;

        public StudentClassroomController(IStudentClassroomService studentClassroomService, LmsDbContext context)
        {
            _studentClassroomService = studentClassroomService;
            _context = context;
        }

        //==========CONTROLLER TO STUDENT JOIN CLASSROOM==========
        [HttpPost("JoinClass-bycode")]

        public async Task<IActionResult> JoinClassroomByCode([FromBody] string inviteCode)
        {
            // 1. Lấy User ID (UUID) từ Token
            var userId = User.Identity?.Name;

            

            
            

            // 3. Sử dụng StudentId (Mã số SV) cho Service Layer
            if (String.IsNullOrEmpty(inviteCode))
                return BadRequest("Invite code is required");

            var classroomId = await _studentClassroomService.JoinClassroomByCodeAsync(inviteCode, userId);

            if (classroomId == null)
            {
                return NotFound("Classroom not found or invite code is invalid/inactive.");
            }

            return Ok(new { ClassroomId = classroomId, Message = "Successfully joined the classroom." });
        }

        [HttpGet("My-Classrooms")]
        public async Task<IActionResult> GetMyClassrooms()
        {
            var userId = User.Identity?.Name;

            if (string.IsNullOrWhiteSpace(userId))
                return Unauthorized("Cannot identify user from token.");

            // Tra cứu Student ID (Mã số SV) từ User ID (UUID) để gọi Service
            var student = await _context.Students
                .FirstOrDefaultAsync(st => st.UserId == userId);


            var result = await _studentClassroomService.GetMyClassroomsAsync(userId);

            // Nếu bạn muốn yêu cầu phải có hồ sơ Student ở đây:
            /*
            if (student == null)
            {
                return StatusCode(403, new { Message = "Không tìm thấy hồ sơ sinh viên." });
            }
            var result = await _studentClassroomService.GetMyClassroomsAsync(student.StudentId); // Cần sửa Service để nhận StudentId
            */

            return Ok(result);
        }

    }
}