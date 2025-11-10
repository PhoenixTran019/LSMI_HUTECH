using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Domain.Entities;
using LmsMini.Application.DTOs;
using Microsoft.EntityFrameworkCore;



namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StudentController : Controller
    {
        private readonly IStudentService _studentService;
        private readonly LmsDbContext _context;

        [Authorize(Roles = "TrainingManager,Admin")]
        [HttpPost("create-student")]
        public async Task<IActionResult> CreateStudent([FromBody]CreateStudentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == userId);

            var success = await _studentService.CreateStudentWithAccountAsync(dto, staff.StaffId);
            if (!success)
            {
                return BadRequest("Student ID already exists or Student role has not been configured.");
            }
            return Ok("Create Student and Student Account Success!");
        }
    }
}
