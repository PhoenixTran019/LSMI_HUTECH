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
    [Route("api/[controller]")]
    public class ClassroomController : Controller
    {
        private readonly IClassroomService _classroomService;
        private readonly LmsDbContext _context;

        public ClassroomController(IClassroomService classroomService, LmsDbContext context)
        {
            _classroomService = classroomService;
            _context = context;
        }

        //Create Classroom
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

            List<string>? allowedDepartIds = null;

            if(role != "Admin")
            {
                //Get StaffId from UserId
                var staffId = await _context.DepartmentStaffs
                    .Where(s => s.UserId == userId)
                    .Select(s => s.StaffId)
                    .FirstOrDefaultAsync();

                //Take list Key of departments that staff manage
                allowedDepartIds = await _context.StaffDeparts
                    .Where(sd => sd.StaffId == staffId)
                    .Select(sd => sd.DepartId)
                    .ToListAsync();
            }
            var result = await _classroomService.GetDashboardClassroomsAsync(filter, allowedDepartIds);
            return Ok(result);
        }

        //Add member by hand
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("classroom/{classroomId}/add-member")]
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
        [HttpPut("classroom/{classroomId}/update-role")]
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
    }
}
