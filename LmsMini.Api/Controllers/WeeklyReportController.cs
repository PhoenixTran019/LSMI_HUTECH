using LmsMini.Application.DTOs.ProjectWeeklyReport;
using LmsMini.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LmsMini.Api.Controllers
{

    [ApiController]
    [Route("api/weeklyProjectReport/")]
    public class WeeklyReportController : Controller
    {
        private readonly ILecWeekReportService _weekReportService;

        public WeeklyReportController(ILecWeekReportService weekReportService)
        {
            _weekReportService = weekReportService;
        }

        [Authorize(Roles ="Admin, Staff, Lecturer")]
        [HttpPost("Weekly-Report/create")]
        public async Task<IActionResult> CreateWeeklyReport([FromBody] CreateLecWeekReportDto dto)
        {
            var lecturerId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            var id = await _weekReportService.CreateLecWeekReportAsync(dto, lecturerId);

            return Ok(new { ReportID = id, Message = "Submit Weekly Report Success" });
        }
    }
}
