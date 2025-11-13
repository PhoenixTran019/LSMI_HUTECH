using LmsMini.Application.DTOs.Classroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

    }
}
