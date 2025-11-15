using LmsMini.Application.DTOs.Lesson;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LessonController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly IWebHostEnvironment _env;
        public LessonController(ILessonService lessonService, IWebHostEnvironment env)
        {
            _lessonService = lessonService;
            _env = env;
        }


        //Create Lesson with file uploads
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("create-lesson")]
        public async Task<IActionResult> CreateLesson([FromForm] CreateLessonWithFilesDto dto)
        {
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var lessonId = await _lessonService.CreateLessonWithFilesAsync(dto, staffId, _env.WebRootPath);
            return Ok(new { LessonID = lessonId });
        }

        //Create Assignment with file uploads
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("create-assignment")]
        public async Task<IActionResult> CreateAssignment([FromForm] CreateAssigmentWithFilesDto dto)
        {
            var teacherId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var assignId = await _lessonService.CreateAssignmentWithFilesAsync(dto, teacherId, _env.WebRootPath);
            return Ok(new { AssignmentID = assignId });
        }
    }
}
