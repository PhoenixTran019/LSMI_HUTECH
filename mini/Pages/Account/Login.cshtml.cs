using LmsMini.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;

namespace LmsMini.WebApp.Pages.Account
{
    [AllowAnonymous]
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;

        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = null,
            PropertyNameCaseInsensitive = true
        };

        // InputModel để validate trên Razor (không phụ thuộc DTO API có DataAnnotations hay không)
        public class LoginInputModel
        {
            [Required(ErrorMessage = "Vui lòng nhập tên đăng nhập")]
            public string UserName { get; set; } = string.Empty;

            [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
            public string Password { get; set; } = string.Empty;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        // Bind ReturnUrl cả GET + POST
        [BindProperty(SupportsGet = true)]
        public string? ReturnUrl { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task OnGetAsync()
        {
            // Nếu đã đăng nhập mà vào trang login thì sign out cho sạch (tuỳ bạn muốn hay không)
            if (User?.Identity?.IsAuthenticated == true)
                await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            ReturnUrl ??= Url.Content("~/");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            ReturnUrl ??= Url.Content("~/");

            if (!ModelState.IsValid)
                return Page();

            // Payload khớp API: LoginDto { UserName, Password }
            var payload = new LoginDto
            {
                UserName = Input.UserName,
                Password = Input.Password
            };

            var client = _httpClientFactory.CreateClient("LmsApi");

            try
            {
                // Gửi đúng endpoint API
                var response = await client.PostAsJsonAsync("api/Account/login", payload, _jsonOptions);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<LoginResponseDto>(_jsonOptions);

                    if (result == null || string.IsNullOrWhiteSpace(result.Token))
                    {
                        ModelState.AddModelError(string.Empty, "API không trả về token hợp lệ.");
                        return Page();
                    }

                    await SignInUser(result);

                    // ***** LOGIC CHUYỂN HƯỚNG ĐÃ SỬA *****
                    string targetUrl = result.RedirectUrl;

                    // Nếu API không trả về RedirectUrl, tự xác định URL dựa trên Role
                    if (string.IsNullOrWhiteSpace(targetUrl))
                    {
                        targetUrl = result.Role switch
                        {
                            "Student" => "/student/dashboard",
                            "Lecturer" => "/lecturer/dashboard",
                            "Staff" => "/staff/dashboard",
                            "Admin" => "/admin/dashboard",
                            _ => ReturnUrl // Fallback về ReturnUrl cũ nếu không xác định được Role
                        };
                    }

                    // Nếu URL đã xác định là Local, chuyển hướng đến đó
                    if (!string.IsNullOrWhiteSpace(targetUrl) && Url.IsLocalUrl(targetUrl))
                    {
                        return LocalRedirect(targetUrl);
                    }

                    // Nếu không có RedirectUrl (hoặc URL không hợp lệ), quay về ReturnUrl gốc
                    return LocalRedirect(ReturnUrl);
                    // ***** KẾT THÚC SỬA *****
                }

                var error = await response.Content.ReadAsStringAsync();
                ModelState.AddModelError(string.Empty, $"Đăng nhập không thành công. {error}");
                return Page();
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi kết nối tới server API: {ex.Message}");
                return Page();
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Lỗi: {ex.Message}");
                return Page();
            }
        }

        private async Task SignInUser(LoginResponseDto result)
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(result.Token);

            var claims = new List<Claim>();

            // 1) Lấy claim từ JWT (nếu có)
            foreach (var c in jwtToken.Claims)
            {
                // Role (nếu token dùng "role")
                if (c.Type == "role" || c.Type == "roles")
                {
                    if (!string.IsNullOrWhiteSpace(c.Value))
                        claims.Add(new Claim(ClaimTypes.Role, c.Value));
                    continue;
                }

                // Map một số claim phổ biến
                var mapped = c.Type switch
                {
                    JwtRegisteredClaimNames.Sub => ClaimTypes.NameIdentifier,
                    "name" => ClaimTypes.Name,
                    "username" => ClaimTypes.Name,
                    _ => c.Type
                };

                if (!claims.Any(x => x.Type == mapped && x.Value == c.Value))
                    claims.Add(new Claim(mapped, c.Value));
            }

            // 2) Bổ sung từ response (để chắc chắn có Name + Role)
            if (!string.IsNullOrWhiteSpace(result.User_name) && !claims.Any(c => c.Type == ClaimTypes.Name))
                claims.Add(new Claim(ClaimTypes.Name, result.User_name));

            // *** Đây là phần quan trọng nhất: Đảm bảo Role được thêm vào Cookie Claims ***
            if (!string.IsNullOrWhiteSpace(result.Role) && !claims.Any(c => c.Type == ClaimTypes.Role))
                claims.Add(new Claim(ClaimTypes.Role, result.Role));
            // *****************************************************************************

            if (!claims.Any(c => c.Type == ClaimTypes.NameIdentifier))
            {
                // fallback: dùng subject nếu có
                if (!string.IsNullOrWhiteSpace(jwtToken.Subject))
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, jwtToken.Subject));
                else if (!string.IsNullOrWhiteSpace(result.User_name))
                    claims.Add(new Claim(ClaimTypes.NameIdentifier, result.User_name));
            }

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            var expiresUtc = jwtToken.ValidTo > DateTime.UtcNow
                ? DateTimeOffset.UtcNow + (jwtToken.ValidTo - DateTime.UtcNow)
                : DateTimeOffset.UtcNow.AddHours(1);

            var props = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = expiresUtc,
                AllowRefresh = true
            };

            // Lưu token vào auth ticket để handler khác lấy ra (AuthenticatedHttpMessageHandler)
            props.StoreTokens(new[]
            {
                new AuthenticationToken { Name = "access_token", Value = result.Token! }
            });

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
        }
    }
}