using LmsMini.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Helper/")]
    public class HelpDropController : Controller
    {
        private readonly IDropHeplerService _dropHeplerService;

        public HelpDropController(IDropHeplerService dropHeplerService)
        {
            _dropHeplerService = dropHeplerService;
        }


        [Authorize(Roles = "Lecturer, Staff")]
        [HttpGet("/weekly-report/groups")]
        public async Task<IActionResult> GetGroupDropDown()
        {
            var lecturerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var groups = await _dropHeplerService.GetGroupsForLecturerAsync(lecturerId);

            return Ok(groups);
        }
    }
}