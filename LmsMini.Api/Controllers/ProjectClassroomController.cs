using LmsMini.Application.DTOs.ProjectClassroom;
using LmsMini.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectClassroomController : Controller
    {
        private readonly IProjectClassroomService _projectClassroomService;

        public ProjectClassroomController (IProjectClassroomService projectClassroomService)
        {
            _projectClassroomService = projectClassroomService;
        }


        //==========CREATE NEW CLASSROOM FOR PROJECT==========
        [Authorize(Roles= "Staff,Lecturer,Admin")]
        [HttpPost("CrateProjectClassroom")]
        public async Task<IActionResult> CreateClassroom([FromBody] CreateProjectClassroomDto dto, string staffId)
        {
            staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            try
            {
                var proClassId = await _projectClassroomService.CreateProjectClassroomAsync(dto, staffId);

                //Return DB to FE
                return Ok(new
                {
                    ProClassId = proClassId,
                    Message = "Project Classroom create success"
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // Có thể log chi tiết ex.Message hoặc stacktrace ở đây
                return StatusCode(500, new { Error = "Có lỗi xảy ra khi tạo Project Classroom." });
            }
        }

        [Authorize(Roles="Staff,Lecturer,Admin")]
        [HttpPost("{proClassID}/Add-Lecturer")]
        public async Task<IActionResult> AddLecturer([FromBody] AddLecturerToProjectClassroomDto dto, string staffId, string proClassID)
        {

            try
            {
                staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                dto.ProClassID = proClassID;

                await _projectClassroomService.AddMemberAsync(dto, staffId);

                return Ok(new
                {
                    Message = "Add new lecturer sucess!",
                    ProClassId = dto.ProClassID,
                    NewLecturerUd = dto.NewMemberLecturerID
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return StatusCode(403, new { Error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { Error = ex.Message });
            }
            catch (Exception)
            {
                return StatusCode(500, new { Error = "Đã xảy ra lỗi hệ thống khi thêm giảng viên vào lớp." });
            }
        }

    }
}
