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
        public async Task<IActionResult> CreateClassroom([FromBody] CreateProjectClassroomDto dto)
        {
            try
            {
                var proClassId = await _projectClassroomService.CreateProjectClassroomAsync(dto);

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
    }
}
