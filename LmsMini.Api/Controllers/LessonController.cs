using LmsMini.Application.DTOs.ClassAssignment;
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
            try
            {
                // Check Staff Login
                var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(staffId))
                    return Unauthorized("Cannot identify staff from token.");

                // Basic validation
                if (string.IsNullOrWhiteSpace(dto.LessonTitle))
                    return BadRequest("Lesson title is required.");

                if (string.IsNullOrWhiteSpace(dto.ClassrooomID))
                    return BadRequest("Classroom ID is required.");

                // Call Service
                var lessonId = await _lessonService.CreateLessonWithFilesAsync(dto, staffId, _env.WebRootPath);

                return Ok(new { LessonID = lessonId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Error creating lesson",
                    Detail = ex.Message
                });
            }
        }

       

        // Get lesson detail
        [Authorize (Roles = "Staff,Lecturer,Admin")]
        [HttpGet("{lessonId}/detail")]
        public async Task<IActionResult> GetLessonDetail(string lessonId)
        {
            var detail = await _lessonService.GetLessonDetailAsync(lessonId);

            if (detail == null) 
                return NotFound("Doesn't find the lesson!");
            
            return Ok(detail);
        }

        

        ///<summary>
        ///Update Lesson (Form-data to sp upload file
        ///FE need to send Classname and LessonName if want to save file right way
        ///RemoveFileName: List of original names (FileName) to delete.
        ///</summary>
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPut("{id}-updateLesson")]
        public async Task<IActionResult> UpdateLesson([FromRoute] string id, [FromForm] UpdateLessonDto dto)
        {
            //Take StaffID form claim
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if(string.IsNullOrEmpty(staffId))
                return Unauthorized("Doesn't find StaffId in token");

            //webRootPath to build phisical link
            var webRootPath = _env.WebRootPath;
            if(string.IsNullOrEmpty(webRootPath))
                return StatusCode(500, "Cann't determine web root path");

            var ok = await _lessonService.UpdateLessonAsync(id, dto, staffId, webRootPath);

            if (!ok)
            {
                return NotFound("Lesson not found!");

                
            }
            return NoContent();
        }

        //Controller to delete lesson
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpDelete("{id}-deleteLesson")]
        public async Task<IActionResult> DeleteLesson([FromRoute] string id)
        {
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(staffId))
                return Unauthorized("Doesn't find StaffId in token");

            var webRootPath = _env.WebRootPath;

            var deteted = await _lessonService.DeleteLessonAsync(id, staffId, webRootPath);
            if (!deteted)
            {
                return NotFound("Lesson not found!");
            }

            return NoContent();
        }
    }
}
