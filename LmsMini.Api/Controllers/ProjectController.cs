using LmsMini.Application.DTOs.Project;
using LmsMini.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectController : Controller
    {

        private readonly IProjectService _projectService;

        public ProjectController(IProjectService projectService)
        {
            _projectService = projectService;
        }

        //==========CONTROLLER TO CREATE PROJECT==========
        [HttpPost("Create-Project")]
        [Authorize(Roles = "Staff, Admin")]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectDto dto)
        {
            if(!ModelState.IsValid)
                return BadRequest(ModelState);

            //Take ID from token
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (staffId == null)
            {
                return Unauthorized("Cannot identify staff from token.");
            }

            try
            {
                var projectId = await _projectService.CreateProjectAsync(dto, staffId);
                return Ok(new
                {
                    Message = "Project created successfully",
                    ProjectId = projectId,
                });
            }
            catch(ArgumentException ex)
            {
                //Error validate input
                return BadRequest(new { Error = ex.Message });
            }
            catch (Exception ex)
            {
                // Lỗi hệ thống
                return StatusCode(500, new { Error = ex.Message });
            }
        }

        //==========APPROVE FOR STAFF===========
        [HttpPost("Staff-Approve")]
        [Authorize(Roles = "Staff, Admin")]
        public async Task<IActionResult> StaffApprove([FromBody] ProjectApprovalDto dto)
        {
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (staffId == null)
                return Unauthorized("Cannot identify staff from token.");

            var result = await _projectService.ApproveAsync(dto, staffId);

            return Ok(new {success =  result});
        }

        //==========INDUSTRY LEADER ONLY===========
        [HttpPost("Leader-Approve")]
        [Authorize(Roles = "Staff, Admin, Lecturer")]
        public async Task<IActionResult> LeaderApprove([FromBody] ProjectApprovalDto dto)
        {
            var staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (staffId == null)
                return Unauthorized("Cannot identify staff from token.");

            var result = await _projectService.ApproveAsync(dto, staffId);

            return Ok(new {success = result});
        }
    }
}
