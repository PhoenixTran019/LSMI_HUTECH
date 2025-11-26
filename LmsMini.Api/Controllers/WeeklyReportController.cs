using LmsMini.Application.DTOs.ProjectClassroom;
using LmsMini.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/classrooms")]
    public class WeeklyReportController : Controller
    {
        private readonly IProWeeklyService _proWeeklyService;

        public WeeklyReportController(IProWeeklyService proWeeklyService)
        {
            _proWeeklyService = proWeeklyService;
        }

        [Authorize(Roles = "Admin, Staff, Lecturer")]
        [HttpPost("{proClassId}/create-Meeting")]
        public async Task<IActionResult> CreateMeeting (string proClassId, CreateMeetingDto dto, string staffId)
        {
            staffId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var meetingId = await _proWeeklyService.CreateMeetingAsync(proClassId, dto, staffId);

            return Ok(meetingId);
        }
    }
}
