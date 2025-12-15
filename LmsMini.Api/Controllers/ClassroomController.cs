using LmsMini.Application.DTOs.Classroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Claims;


namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Classroom")]
    public class ClassroomController : Controller
    {
        private readonly IClassroomService _classroomService;
        private readonly LmsDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ClassroomController(IClassroomService classroomService, LmsDbContext context, IWebHostEnvironment env)
        {
            _classroomService = classroomService;
            _context = context;
            _env = env;
        }

        //==========Create Classroom=========
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("create-classroom")]
        public async Task<IActionResult> CreateClassroom([FromBody] CreateClassroomDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == userId);
            if (staff == null) return Forbid("The employee performing the task could not be identified.");

            var success = await _classroomService.CreateClassroomAsync(dto, staff.StaffId);
            if (!success)
            {
                return BadRequest("Classroom name is exist or incorrect database!");
            }
            return Ok("Create Classroom Success!");
        }


        //View Classroom
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpGet("dashboard-classrooms")]
        public async Task<IActionResult> GetClassromDashboard([FromQuery] ClassroomFilterDto filter)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Cannot identify user from token.");
            }

            // Loại bỏ logic lọc theo Department ở Controller
            // Thay vào đó, truyền role và userId để Service tự quyết định cách lọc
            var result = await _classroomService.GetDashboardClassroomsAsync(filter, role, userId);

            return Ok(result);
        }

        //Add member by hand
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("{classroomId}/add-member")]
        public async Task<IActionResult> AddMember(string classroomId, [FromBody]AddMemberDto dto)
        {
            var currentUserId = User.FindFirst (ClaimTypes.NameIdentifier)?.Value;

            var isTeacher = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId &&
                               m.LecturerId == currentUserId &&
                               m.RoleInClass == "Teacher");

            if(!isTeacher) return Forbid("Only teacher can add member to class.");

            var success = await _classroomService.AddMemberToClassroomAsync(classroomId, dto.UserId, dto.Role);
            return success ? Ok() : BadRequest("User already exists in classroom.");
        }

        //chage roll in class
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPut("{classroomId}/update-role")]
        public async Task<IActionResult> UpdateRole (string classroomId, [FromBody] UpdateRoleDto dto)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var isTeacher = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId &&
                               m.LecturerId == currentUserId &&
                               m.RoleInClass == "Teacher");

            if (!isTeacher) return Forbid("Only Teachers can update roles.");

            var succes = await _classroomService.UpdateMemberRoleAsync(classroomId, dto.UserId, dto.NewRole);
            return succes ? Ok() : BadRequest("Member not found");
        }

        //==========Get view  lesson and assignment in classroom==========
        [Authorize]
        [HttpGet("{classroomId}/Classroom-Dashboard-Detail")]
        public async Task<IActionResult> GetClassroomOverview (string classroomId)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                // Có thể xảy ra nếu token có vấn đề nhưng đã vượt qua Authorize
                return Unauthorized("User identity or role is missing.");
            }

            // Truyền userId và role vào Service
            var result = await _classroomService.GetOverviewAsync(classroomId, userId, role);

            if (result == null)
                return NotFound("Classroom not found.");

            return Ok(result);
        }

        [Authorize(Roles ="Staff, Lecturer, Admin")]
        [HttpPut("{classroomId}/Update-Classroom")]
        public async Task<IActionResult> UpdateClassroom(string classroomId, [FromBody] UpdateClassroomDto dto)
        {
            var staffId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffId))
                return Unauthorized("Cannot identify staff from token.");

            var ok = await _classroomService.UpdateClassroomAsync(classroomId, dto, staffId);

            if (!ok)
                return NotFound("Classroom not found or failed to update");

            return NoContent();
        }

        [Authorize(Roles = ("Admin, Staff"))]
        [HttpDelete("{classroomId}/Delete-Classroom")]
        public async Task<IActionResult> DeleteClassroom (string classroomId)
        {
            var staffId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(staffId))
                return Unauthorized("Cannot identify staff form token");

            var webRootPath = _env.WebRootPath;

            var ok = await _classroomService.DeleteClassroomAsync(classroomId, staffId, webRootPath);

            if (!ok)
                return NotFound("Class not found or cannot delete");

            return NoContent();
        }
    }
}
