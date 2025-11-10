using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string Token { get; set; }
        public string Role { get; set; }
        public string RedirectUrl { get; set; }
    }
}
