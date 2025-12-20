//using LmsMini.Application.DTOs.Project;
using LmsMini.WebApp.ApiClients.Dto.Project;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace LmsMini.WebApp.ApiClients
{
    public class ProjectApiClient : IProjectApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        // Constructor giả định đã được cấu hình trong Program.cs để nhận Named Client
        public ProjectApiClient(HttpClient http)
        {
            _http = http;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // 1. TẠO DỰ ÁN
        public async Task<(bool IsSuccess, string? Error, string? ProjectId)> CreateProjectAsync(CreateProjectDto dto)
        {
            try
            {
                // Gửi DTO dưới dạng JSON
                var resp = await _http.PostAsJsonAsync("api/Project/Create-Project", dto);
                var body = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                {
                    // Giả định API trả về { ProjectId: "..." }
                    using var doc = JsonDocument.Parse(body);
                    string? projectId = null;
                    if (doc.RootElement.TryGetProperty("projectId", out var projectIdElement))
                    {
                        projectId = projectIdElement.GetString();
                    }
                    return (true, null, projectId);
                }

                // Xử lý lỗi từ BadRequest hoặc StatusCode 500
                return (false, body, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}", null);
            }
        }

        // 2. PHÊ DUYỆT (Staff/Admin)
        public async Task<(bool IsSuccess, string? Error)> StaffApproveAsync(ProjectApprovalDto dto)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/Project/Staff-Approve", dto);

                if (resp.IsSuccessStatusCode) return (true, null);

                var errorBody = await resp.Content.ReadAsStringAsync();
                return (false, errorBody);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        // 3. PHÊ DUYỆT (Lecturer/Admin)
        public async Task<(bool IsSuccess, string? Error)> LeaderApproveAsync(ProjectApprovalDto dto)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/Project/Leader-Approve", dto);

                if (resp.IsSuccessStatusCode) return (true, null);

                var errorBody = await resp.Content.ReadAsStringAsync();
                return (false, errorBody);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }
    }
}