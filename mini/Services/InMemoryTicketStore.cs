using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Caching.Memory;

namespace LmsMini.WebApp.Services
{
    // Class này chịu trách nhiệm lưu thông tin đăng nhập vào RAM server
    // Giúp Cookie trình duyệt cực nhỏ, tránh lỗi HTTP 400 Bad Request
    public class InMemoryTicketStore : ITicketStore
    {
        private readonly IMemoryCache _cache;
        private const string KeyPrefix = "AuthSession-";

        public InMemoryTicketStore(IMemoryCache cache)
        {
            _cache = cache;
        }

        public async Task<string> StoreAsync(AuthenticationTicket ticket)
        {
            var key = KeyPrefix + Guid.NewGuid().ToString();
            await RenewAsync(key, ticket);
            return key;
        }

        public Task RenewAsync(string key, AuthenticationTicket ticket)
        {
            var options = new MemoryCacheEntryOptions();
            var expiresUtc = ticket.Properties.ExpiresUtc;

            if (expiresUtc.HasValue)
            {
                options.SetAbsoluteExpiration(expiresUtc.Value);
            }

            // Gia hạn thời gian sống nếu người dùng còn thao tác
            options.SetSlidingExpiration(TimeSpan.FromHours(1));

            _cache.Set(key, ticket, options);
            return Task.CompletedTask;
        }

        public Task<AuthenticationTicket?> RetrieveAsync(string key)
        {
            _cache.TryGetValue(key, out AuthenticationTicket? ticket);
            return Task.FromResult(ticket);
        }

        public Task RemoveAsync(string key)
        {
            _cache.Remove(key);
            return Task.CompletedTask;
        }
    }
}