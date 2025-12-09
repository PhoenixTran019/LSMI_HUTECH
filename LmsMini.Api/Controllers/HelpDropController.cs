using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Helper/")]
    public class HelpDropController : Controller
    {
        private readonly IDropHeplerService _dropHeplerService;
        private readonly LmsDbContext _context;

        public HelpDropController(IDropHeplerService dropHeplerService, LmsDbContext context)
        {
            _dropHeplerService = dropHeplerService;
            _context = context;
        }

        
        [Authorize(Roles ="Lecturer, Staff")]
        [HttpGet("weekly-report/groups")]
        public async Task<IActionResult> GetGroupDropDown()
        {
            var lecturerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var groups = await _dropHeplerService.GetGroupsForLecturerAsync(lecturerId);

            return Ok (groups); 
        }

        [Authorize (Roles = "Staff, Admin, Lecturer")]
        [HttpGet("Classroom/Subject")]
        public async Task <IActionResult> GetClassSubDrop()
        {
            var ClassSub = await _context.Subjects
                .Select(s => new { s.SubId, s.SubName })
                .ToListAsync();
            return Ok(ClassSub);

        }

        [Authorize(Roles = "Staff, Admin, Lecturer")]
        [HttpGet("Clasroom/MainClass")]
        public async Task <IActionResult> GetMainClass()
        {
            var mainClass = await _context.Classes
                .Select(m => new { m.ClassId, m.ClassName })
                .ToListAsync();
            return Ok(mainClass);
        }

    }
}
