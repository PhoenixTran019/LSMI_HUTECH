using LmsMini.Domain.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LmsMini.Application.Interfaces;
using LmsMini.Application.DTOs.Auth;
using Microsoft.EntityFrameworkCore;

namespace LmsMini.Api.Controllers
{
    /// <summary>
    /// Controller xử lý tài khoản: đăng ký và đăng nhập.
    /// </summary>
    /// <remarks>
    /// Quy trình đăng nhập (tóm tắt):
    /// - Kiểm tra request.
    /// - Tìm user bằng email và kiểm tra mật khẩu.
    /// - Lấy roles và tạo claims cho JWT.
    /// - Lấy khóa JWT từ cấu hình (Jwt:Key); nếu thiếu thì trả lỗi server.
    /// - Chuyển khóa sang bytes, tạo SymmetricSecurityKey và SigningCredentials.
    /// - Xây dựng JwtSecurityToken (issuer, audience, claims, expiry) và trả token.
    /// - Tránh truyền chuỗi null cho Encoding.UTF8.GetBytes để tránh CS8604.
    /// </remarks>
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase // Inherit from ControllerBase to access BadRequest and other helper methods
    {
        
        
        private readonly IJwtService _jwtService;

        private readonly LmsDbContext _context;

        

        public AccountController(LmsDbContext context, IJwtService jwtService)
        {
            _jwtService = jwtService;
            _context = context;
        }

        

        // dang nhap
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto login)
        {
            var user = await _context.Users
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == login.UserName);

            if(user == null)
                return Unauthorized("Invalid username or password");

            var hasedInput = _jwtService.HashPassword(login.Password);

            if(user.PasswordHash != hasedInput)
                return Unauthorized("Invalid username or password");

            var token = _jwtService.CreateToken(user, user.Role.RoleName);

            var redirectUrl = user.Role.RoleName switch
            {
                "Admin" => "/admin/dashboard",
                "Staff" => "/staff/dashboard",
                "Student" => "/student/dashboard",
                _ => "/home"
            };
            return Ok(new LoginResponseDto
            {
                Token = token,
                Role = user.Role.RoleName,
                RedirectUrl = redirectUrl
            });
        }
    }
}
