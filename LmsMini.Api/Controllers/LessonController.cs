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
        private readonly IWebHostEnvironment _env;
        private readonly LmsDbContext _context;

        public LessonController(ILessonService lessonService, IWebHostEnvironment env, LmsDbContext context)
        {
            _lessonService = lessonService;
            _env = env;
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
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == userId);
                if (staff == null)
                {
                    
                    return StatusCode(403, new
                    {
                        Message = "Không thể xác định nhân viên thực hiện tác vụ này trong hệ thống nghiệp vụ."
                    });
                }

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
                var lessonId = await _lessonService.CreateLessonWithFilesAsync(dto, staff.StaffId, _env.WebRootPath);

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
            var staffId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == staffId);

            if (staff == null)
            {
                
                return StatusCode(403, new
                {
                    Message = "Không thể xác định nhân viên thực hiện tác vụ này trong hệ thống nghiệp vụ."
                });
            }

            //webRootPath to build phisical link
            var webRootPath = _env.WebRootPath;
            if(string.IsNullOrEmpty(webRootPath))
                return StatusCode(500, "Cann't determine web root path");

            var ok = await _lessonService.UpdateLessonAsync(classroomId, lessonId, dto, staff.StaffId, webRootPath);

            if (!ok)
            {
                return NotFound("Lesson not found!");

                
            }
            return NoContent();
        }

        //Controller to delete lesson
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpDelete("{lessonId}/Delete-Lesson")]
        public async Task<IActionResult> DeleteLesson(string classroomId, string lessonId)
        {
            var staffId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == staffId);

            if (staff == null)
            {
                
                return StatusCode(403, new
                {
                    Message = "Không thể xác định nhân viên thực hiện tác vụ này trong hệ thống nghiệp vụ."
                });
            }

            if (string.IsNullOrEmpty(staffId))
                return Unauthorized("Doesn't find StaffId in token");

            var webRootPath = _env.WebRootPath;

            var deteted = await _lessonService.DeleteLessonAsync(classroomId, lessonId, staff.StaffId, webRootPath);
            if (!deteted)
            {
                return NotFound("Lesson not found!");
            }

            return NoContent();
        }

      
    }
}
