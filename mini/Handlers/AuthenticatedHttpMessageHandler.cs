using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Authentication; // <<< CẦN THÊM USING NÀY

namespace LmsMini.WebApp.Handlers
{
    /// <summary>
    /// DelegatingHandler dùng để forward token (hoặc cookie) từ HttpContext -> Authorization header khi gọi API.
    /// </summary>
    public class AuthenticatedHttpMessageHandler : DelegatingHandler
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthenticatedHttpMessageHandler(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) // <<< THÊM async
        {
            var token = await GetTokenFromContextAsync(); // <<< SỬA: Chuyển sang gọi Async
            if (!string.IsNullOrEmpty(token))
            {
                // set Bearer token header
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            return await base.SendAsync(request, cancellationToken); // <<< THÊM await
        }

        // SỬA: Phương thức chuyển sang async để dùng GetTokenAsync
        private async Task<string?> GetTokenFromContextAsync()
        {
            var ctx = _httpContextAccessor.HttpContext;
            if (ctx == null) return null;

            // Lấy token được lưu trong Authentication Properties
            // Tên "access_token" là key mặc định/quy ước khi bạn đăng nhập qua Cookie scheme
            var accessToken = await ctx.GetTokenAsync("access_token"); // <<< ĐIỂM CHÍNH ĐÃ SỬA

            if (!string.IsNullOrEmpty(accessToken))
                return accessToken;

            // Nếu bạn có fallback (ví dụ đọc từ cookie cũ), có thể giữ lại:
            // if (ctx.Request.Cookies.TryGetValue("AccessToken", out var token))
            //     return token;

            return null;
        }

        // (Optional) helper to read cookie by name - KHÔNG CẦN THIẾT
    }
}