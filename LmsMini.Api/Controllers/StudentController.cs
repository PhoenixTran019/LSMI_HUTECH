using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
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

        public StudentController(IStudentService studentService, LmsDbContext context)
        {
            _studentService = studentService;
            _context = context;
        }


        //Cotroller Create Student and Student Account
        [Authorize(Roles = "Staff,Admin")]
        [HttpPost("create-student")]
        public async Task<IActionResult> CreateStudent([FromBody] CreateStudentDto dto)
        {

            //Take StaffId from logged in user Token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID not found in token.");
            }

            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == userId);

            //Call Service to create Student and Account
            var success = await _studentService.CreateStudentWithAccountAsync(dto, staff.StaffId);
            if (!success)
            {
                return BadRequest("Student ID already exists or Student role has not been configured.");
            }
            return Ok("Create Student and Student Account Success!");
        }

        //Take Class list for Dropdown in Create Student Form
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses()
        {
            var classes = await _context.Classes
                .Select(c => new { c.ClassId, c.ClassName })
                .ToListAsync();
            return Ok(classes);
        }

        //Take Department list for Dropdown in Create Student Form
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("departments")]
        public async Task<IActionResult> GetDepartments()
        {
            var departments = await _context.Departments
                .Select(d => new { d.DepartId, d.DepartName })
                .ToListAsync();
            return Ok(departments);
        }

        //Take Major list for Dropdown in Create Student Form
        [Authorize(Roles = "Staff,Admin")]
        [HttpGet("majors")]
        public async Task<IActionResult> GetMajors()
        {
            var majors = await _context.Majors
                .Select(m => new { m.MajorId, m.MajorName })
                .ToListAsync();
            return Ok(majors);
        }

    }
}
