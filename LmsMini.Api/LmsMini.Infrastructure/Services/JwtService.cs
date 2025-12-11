using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using LmsMini.Application.Auth; 
namespace LmsMini.Infrastructure.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly JwtOptions _opts ;
        
        public JwtService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string CreateToken(User user, string roleName)
        {

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId),

                new Claim(ClaimTypes.Name, user.Username?? string.Empty),
                
                //Take ID together for any user -> dentifin by BE PersonType
                new Claim("userId", user.UserId),

                //Adding to claim StaffID
                new Claim(ClaimTypes.Role, roleName),

                new Claim("personType", user.Role.RoleName ?? string.Empty),
            };


            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_opts.Key));

            // Tạo SigningCredentials sử dụng HMAC SHA256
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Tạo token với issuer, audience, claims (bao gồm role claims), thời hạn và signing credentials
            var token = new JwtSecurityToken(
                issuer: _opts.Issuer,
                audience: _opts.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: creds
            );

            // Chuyển JwtSecurityToken thành chuỗi JWT đã ký và trả về
            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        /// <summary>
        /// Xác thực một token JWT và trả về <see cref="ClaimsPrincipal"/> nếu token hợp lệ.
        /// </summary>
        /// <param name="token">Chuỗi token JWT cần xác thực.</param>
        /// <returns>
        /// Một <see cref="ClaimsPrincipal"/> chứa các claim nếu token hợp lệ;
        /// trả về null nếu token không hợp lệ hoặc đã hết hạn.
        /// </returns>

        public ClaimsPrincipal? ValidateToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return null;

            var key = _configuration["Jwt:Key"];
            var issuer = _configuration["Jwt:Issuer"];
            var audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrWhiteSpace(key))
                return null;

            var tokenHandler = new JwtSecurityTokenHandler();
            var keyBytes = Encoding.UTF8.GetBytes(key);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(keyBytes),

                ValidateIssuer = !string.IsNullOrWhiteSpace(issuer),
                ValidIssuer = issuer,

                ValidateAudience = !string.IsNullOrWhiteSpace(audience),
                ValidAudience = audience,

                ClockSkew = TimeSpan.FromMinutes(1)
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        public string HashPassword(string password)
        {
            if (password == null)
                throw new ArgumentNullException(nameof(password));

            using var sha256 = SHA256.Create();
            var bytes = Encoding.UTF8.GetBytes(password);
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
