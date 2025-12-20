using System;

namespace LmsMini.Application.DTOs.Auth
{
    // Minimal DTOs required by the Razor Pages Login page.
    public class LoginDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginResponseDto
    {
        public string? User_name { get; set; }
        public string? Token { get; set; }
        public string? Role { get; set; }
        public string? RedirectUrl { get; set; }
    }
}
