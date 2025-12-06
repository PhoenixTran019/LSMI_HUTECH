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
        private readonly IHttpContextAccessor _http;

        public WeeklyReportController(ILecWeekReportService weekReportService, IHttpContextAccessor http)
        {
            _weekReportService = weekReportService;
            

        }


        [Authorize(Roles ="Admin, Staff, Lecturer")]
        [HttpPost("LecCreate")]
        public async Task<IActionResult> CreateWeeklyReport([FromBody] CreateLecWeekReportDto dto)
        {
            var lecturerId = User.FindFirst("userID")?.Value;

            var id = await _weekReportService.CreateLecWeekReportAsync(dto, lecturerId);

            return Ok(new { ReportID = id, Message = "Submit Weekly Report Success" });
        }

        [Authorize]
        [HttpGet("Dashboard")]
        public async Task<IActionResult> GetDashboard()
        {
            var userName = User.FindFirst("userId")?.Value;
            var personType = User.FindFirst("personType")?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.ValueType;

            var data = await _weekReportService.GetWeekReportDashboardAsync(userName, personType, role);

            return Ok(data);
        }

        [Authorize]
        [HttpGet("Detail/{reportId}")]
        public async Task<IActionResult> GetReportDetail(string reportId)
        {
            var result = await _weekReportService.GetWeekReportDetailAsync(reportId);

            return Ok(result);
        }
    }
}
