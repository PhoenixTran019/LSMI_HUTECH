using LmsMini.Application.DTOs.Classroom;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using System.Security.Claims;


namespace LmsMini.Api.Controllers
{
    [ApiController]
    [Route("api/Classroom")]
    public class ClassroomController : Controller
    {
        private readonly IClassroomService _classroomService;
        private readonly LmsDbContext _context;
        private readonly string _uploadRoot;

        public ClassroomController(IClassroomService classroomService, LmsDbContext context, IConfiguration configuration)
        {
            _classroomService = classroomService;
            _context = context;
            _uploadRoot = configuration["UploadSettings:RootPath"];
        }

        //==========Create Classroom=========
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("create-classroom")]
        public async Task<IActionResult> CreateClassroom([FromBody] CreateClassroomDto dto)
        {
            var userId = User.Identity?.Name;
            var staff = await _context.DepartmentStaffs.FirstOrDefaultAsync(s => s.UserId == userId);

            var success = await _classroomService.CreateClassroomAsync(dto, userId);
            if (!success)
            {
                return BadRequest("Classroom name is exist or incorrect database!");
            }
            return Ok("Create Classroom Success!");
        }


        //View Classroom
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpGet("dashboard-classrooms")]
        public async Task<IActionResult> GetClassromDashboard([FromQuery] ClassroomFilterDto filter)
        {
            var userId = User.Identity?.Name;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("Cannot identify user from token.");
            }

            // Loại bỏ logic lọc theo Department ở Controller
            // Thay vào đó, truyền role và userId để Service tự quyết định cách lọc
            var result = await _classroomService.GetDashboardClassroomsAsync(filter, role, userId);

            return Ok(result);
        }

        //Add member by hand
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPost("{classroomId}/add-member")]
        public async Task<IActionResult> AddMember(string classroomId, [FromBody]AddMemberDto dto)
        {
            var currentUserId = User.Identity?.Name;
            if (string.IsNullOrEmpty(currentUserId)) return Unauthorized("Cannot identify user from token.");

            var isAdmin = User.IsInRole("Admin");

            // 1. KIỂM TRA QUYỀN (Logic giữ lại tại Controller, như yêu cầu của bạn)
            var isTeacher = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId &&
                               m.LecturerId == currentUserId &&
                               m.RoleInClass == "Teacher"); // Lỗi: Teacher phải là hằng số hoặc enum

            if (!isTeacher && !isAdmin) return Forbid("Only a teacher of this class can add members.");

            // 2. Gọi Service và Xử lý lỗi chi tiết
            try
            {
                var success = await _classroomService.AddMemberToClassroomAsync(classroomId, dto.UserId, dto.Role);

                // Service trả về false chỉ khi thành viên đã tồn tại
                return success ? Ok() : BadRequest($"User '{dto.UserId}' already exists in the classroom.");
            }
            catch (KeyNotFoundException ex)
            {
                // Lỗi 400 Bad Request: ID không hợp lệ hoặc Lớp không tồn tại
                return BadRequest(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                // Lỗi 400 Bad Request: Cố gắng thêm vai trò không được phép
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                // Lỗi 500: Lỗi không mong muốn khác
                return StatusCode(500, "An unexpected internal error occurred during member addition.");
            }
        }

        [Authorize]
        [HttpGet("{classroomId}/members")]
        public async Task<IActionResult> GetClassroomMembers (string classroomId)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            var members = await _classroomService.GetClassroomMembersAsync(classroomId);

            if (members == null || !members.Any())
            {
                // Có thể lớp học không tồn tại hoặc không có thành viên
                return NotFound("Classroom not found or no members available.");
            }

            return Ok(members);
        }

        //chage roll in class
        [Authorize(Roles = "Staff,Lecturer,Admin")]
        [HttpPut("{classroomId}/update-role")]
        public async Task<IActionResult> UpdateRole (string classroomId, [FromBody] UpdateRoleDto dto)
        {
            if (!await IsCurrentUserClassTeacher(classroomId))
            {
                return Forbid("Only Teachers can update roles.");
            }

            var success = await _classroomService.UpdateMemberRoleAsync(classroomId, dto.UserId, dto.NewRole);
            return success ? Ok() : BadRequest("Member not found or role update failed.");
        }

        //==========Get view  lesson and assignment in classroom==========
        [Authorize]
        [HttpGet("{classroomId}/Classroom-Dashboard-Detail")]
        public async Task<IActionResult> GetClassroomOverview (string classroomId)
        {

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(ClaimTypes.Role)?.Value;

            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(role))
            {
                // Có thể xảy ra nếu token có vấn đề nhưng đã vượt qua Authorize
                return Unauthorized("User identity or role is missing.");
            }

            // Truyền userId và role vào Service
            var result = await _classroomService.GetOverviewAsync(classroomId, userId, role);

            if (result == null)
                return NotFound("Classroom not found.");

            return Ok(result);
        }

        [Authorize(Roles ="Staff, Lecturer, Admin")]
        [HttpPut("{classroomId}/Update-Classroom")]
        public async Task<IActionResult> UpdateClassroom(string classroomId, [FromBody] UpdateClassroomDto dto)
        {
            var staffId = User.Identity?.Name;
            if (string.IsNullOrEmpty(staffId))
                return Unauthorized("Cannot identify staff from token.");

            var ok = await _classroomService.UpdateClassroomAsync(classroomId, dto, staffId);

            if (!ok)
                return NotFound("Classroom not found or failed to update");

            return NoContent();
        }

        [Authorize(Roles = "Admin, Staff")]
        [HttpDelete("{classroomId}/Delete-Classroom")]
        public async Task<IActionResult> DeleteClassroom(string classroomId)
        {
            // 1. Lấy StaffId (UserId) từ Token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized("Cannot identify staff from token");

            // 2. Tìm StaffId thực tế trong DB (nếu UserId trong token là UUID)
            var staff = await _context.DepartmentStaffs
                .FirstOrDefaultAsync(s => s.UserId == userId);

            var staffId = staff?.StaffId ?? userId;

            // 3. Gọi Service xử lý (Không truyền webRootPath nữa)
            var ok = await _classroomService.DeleteClassroomAsync(classroomId, staffId);

            if (!ok)
                return NotFound("Class not found or cannot delete");

            // 4. Trả về thành công
            return NoContent();
        }

        private async Task<bool> IsCurrentUserClassTeacher(string classroomId)
        {
            var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(currentUserId)) return false;

            // Chuyển việc kiểm tra này vào Service (tốt nhất) hoặc giữ nguyên
            // Nếu bạn giữ nguyên, ít nhất là logic được đặt trong một hàm riêng.
            return await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroomId &&
                               m.LecturerId == currentUserId &&
                               m.RoleInClass == "Teacher");
        }
    }
}
