using LmsMini.Application.DTOs.ClassAssignment;
using LmsMini.Application.DTOs.Lesson;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Classroom/{classroomId}/Lessons")]
    public class LessonController : Controller
    {
        private readonly ILessonService _lessonService;
        private readonly LmsDbContext _context;

        public LessonController(ILessonService lessonService, IWebHostEnvironment env, LmsDbContext context)
        {
            _lessonService = lessonService;
            _context = context;
        }


        //==========Create Lesson with file uploads============
        // LmsMini.Api.Controllers.LessonController.cs

        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("create-lesson")]
        public async Task<IActionResult> CreateLesson([FromForm] CreateLessonWithFilesDto dto, [FromRoute] string classroomId)
        {
            try
            {
                // BƯỚC 1: LẤY STAFFID THỰC TỪ TOKEN CLAIM (Dùng ClaimTypes.Name)
                // Vì bạn xác nhận StaffId TRÙNG VỚI Username, và Username nằm trong claim ClaimTypes.Name
                var userId = User.Identity?.Name;
               
                // BƯỚC 2: KIỂM TRA TÍNH HỢP LỆ CỦA STAFFID TRONG DB
                // Việc này là cần thiết để đảm bảo StaffId này tồn tại trong bảng DepartmentStaffs (tránh lỗi FK)




                // BƯỚC 3: Xử lý logic tạo Lesson
                dto.ClassrooomID = classroomId;

                // Basic validation (giữ nguyên)
                if (string.IsNullOrWhiteSpace(dto.LessonTitle))
                    return BadRequest("Tiêu đề bài giảng là bắt buộc.");

                if (string.IsNullOrWhiteSpace(dto.ClassrooomID))
                    return BadRequest("ID lớp học là bắt buộc.");

                // BƯỚC 4: Gọi Service với StaffId HỢP LỆ (đã fix)
                var lessonId = await _lessonService.CreateLessonWithFilesAsync(dto, userId);

                return Ok(new { LessonID = lessonId });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    Message = "Lỗi khi tạo bài giảng.",
                    Detail = ex.Message
                });
            }
        }



        //==========Get lesson detail==========
        [Authorize]
        [HttpGet("{lessonId}/detail")]
        public async Task<IActionResult> GetLessonDetail(string classroomId,string lessonId)
        {
            var detail = await _lessonService.GetLessonDetailAsync(classroomId, lessonId);

            if (detail == null) 
                return NotFound("Doesn't find the lesson!");

            if (detail.ClassroomID != classroomId)
                return Forbid("Lesson does not belong to this classroom");

            return Ok(detail);
        }

        

        ///<summary>
        ///Update Lesson (Form-data to sp upload file
        ///FE need to send Classname and LessonName if want to save file right way
        ///RemoveFileName: List of original names (FileName) to delete.
        ///</summary>
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPut("{lessonId}/Update-Lesson")]
        public async Task<IActionResult> UpdateLesson(string lessonId, string classroomId, [FromForm] UpdateLessonDto dto)
        {
            //Take StaffID form claim
            var userId = User.Identity?.Name;

            var ok = await _lessonService.UpdateLessonAsync(classroomId, lessonId, dto, userId);

            return ok ? NoContent() : NotFound();
        }

        //Controller to delete lesson
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpDelete("{lessonId}/Delete-Lesson")]
        public async Task<IActionResult> DeleteLesson(string classroomId, string lessonId)
        {
            var userId = User.Identity?.Name;

            var ok = await _lessonService.DeleteLessonAsync(classroomId, lessonId, userId);
            return ok ? NoContent() : NotFound();
        }

      
    }
}
