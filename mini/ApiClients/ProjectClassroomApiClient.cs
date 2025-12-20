using LmsMini.WebApp.ApiClients.Dto.ProjectClassroom;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http; // Cần cho IFormFile
using System.Net.Http.Headers;

namespace LmsMini.WebApp.ApiClients
{
    public class ProjectClassroomApiClient : IProjectClassroomApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public ProjectClassroomApiClient(HttpClient http)
        {
            _http = http;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- HELPER CHO MULTIPART FORM DATA (Tương tự LessonApiClient) ---
        private MultipartFormDataContent CreateMultipartContent(
            object dto,
            List<IFormFile>? newFiles,
            List<string>? filesToDelete = null)
        {
            var content = new MultipartFormDataContent();

            // 1. Thêm các trường dữ liệu text
            var properties = dto.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var propType = prop.PropertyType;

                // Bỏ qua IFormFile List hoặc String List
                if (propType == typeof(List<IFormFile>)) continue;

                var value = prop.GetValue(dto);
                if (value != null)
                {
                    string stringValue = value is DateTime dt ? dt.ToString("yyyy-MM-ddTHH:mm:ss") : value.ToString() ?? "";

                    if (!string.IsNullOrEmpty(stringValue))
                    {
                        content.Add(new StringContent(stringValue), $"\"{prop.Name}\"");
                    }
                }
            }

            // 2. Thêm file mới
            if (newFiles != null)
            {
                foreach (var file in newFiles)
                {
                    if (file.Length > 0)
                    {
                        var fileStreamContent = new StreamContent(file.OpenReadStream());
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        // Tên field phải khớp với DTO API (Ví dụ: "Files" hoặc "NewFiles")
                        content.Add(fileStreamContent, "Files", file.FileName);
                    }
                }
            }

            // 3. Thêm danh sách file cần xóa (nếu có)
            if (filesToDelete != null)
            {
                // API Controller nhận List<string> FilesToDelete (hoặc RemoveFileId)
                // Gửi từng phần tử trong danh sách filesToDelete
                foreach (var fileId in filesToDelete)
                {
                    content.Add(new StringContent(fileId), $"FilesToDelete");
                }
            }

            return content;
        }

        // 1. TẠO PROJECT CLASSROOM
        public async Task<(bool IsSuccess, string? Error, string? ProClassId)> CreateProjectClassroomAsync(
            CreateProjectClassroomRequest dto)
        {
            try
            {
                // URL: api/ProjectClassroom/CrateProjectClassroom
                var resp = await _http.PostAsJsonAsync("api/ProjectClassroom/CrateProjectClassroom", dto);
                var body = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                {
                    // Giả định API trả về { ProClassId: "..." }
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("proClassId", out var idElement))
                    {
                        return (true, null, idElement.GetString());
                    }
                    return (true, null, null);
                }

                return (false, body, null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}", null);
            }
        }

        // 2. LẤY DANH SÁCH LỚP HỌC CỦA STAFF
        public async Task<List<MyProjectClassroomItem>> GetMyClassroomsAsync()
        {
            try
            {
                // URL: api/ProjectClassroom/project-classroom-homepage
                var resp = await _http.GetAsync("api/ProjectClassroom/project-classroom-homepage");
                resp.EnsureSuccessStatusCode();

                var classrooms = await resp.Content.ReadFromJsonAsync<List<MyProjectClassroomItem>>(_jsonOptions);
                return classrooms ?? new List<MyProjectClassroomItem>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API CLIENT ERROR] Lỗi khi lấy danh sách lớp học dự án: {ex.Message}");
                return new List<MyProjectClassroomItem>();
            }
        }

        // 3. THÊM GIẢNG VIÊN VÀO LỚP
        public async Task<(bool IsSuccess, string? Error)> AddLecturerAsync(
            string proClassID,
            AddLecturerRequest dto)
        {
            try
            {
                // URL: api/ProjectClassroom/{proClassID}/Add-Lecturer
                // DTO này cần được gửi dưới dạng JSON Body
                var resp = await _http.PostAsJsonAsync($"api/ProjectClassroom/{proClassID}/Add-Lecturer", dto);

                if (resp.IsSuccessStatusCode) return (true, null);

                var errorBody = await resp.Content.ReadAsStringAsync();
                return (false, errorBody);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        // 4. TẠO NỘI DUNG/BÀI TẬP
        public async Task<(bool IsSuccess, string? Error)> CreateProjectContentAsync(
            string proClassID,
            CreateProjectContentRequest dto)
        {
            try
            {
                using var content = CreateMultipartContent(dto, dto.Files);

                // URL: api/ProjectClassroom/{proClassID}/Create-Content
                var resp = await _http.PostAsync($"api/ProjectClassroom/{proClassID}/Create-Content", content);

                if (resp.IsSuccessStatusCode) return (true, null);

                var body = await resp.Content.ReadAsStringAsync();
                return (false, body);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        // 5. LẤY CHI TIẾT NỘI DUNG/BÀI TẬP
        public async Task<(ProjectContentDetailDto? Detail, string? Error)> GetProjectContentDetailAsync(
            string proClassID,
            string contentId)
        {
            try
            {
                // URL: api/ProjectClassroom/{proClassId}/Content-Detail/{contentId}
                var resp = await _http.GetAsync($"api/ProjectClassroom/{proClassID}/Content-Detail/{contentId}");

                if (!resp.IsSuccessStatusCode)
                {
                    return (null, $"Lỗi HTTP {(int)resp.StatusCode}: {resp.ReasonPhrase}");
                }

                var detail = await resp.Content.ReadFromJsonAsync<ProjectContentDetailDto>(_jsonOptions);
                return (detail, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi kết nối hoặc Deserialize: {ex.Message}");
            }
        }

        // 6. CẬP NHẬT NỘI DUNG/BÀI TẬP
        public async Task<(bool IsSuccess, string? Error)> UpdateProjectContentAsync(
            string proClassID,
            string contentId,
            ProContentUpdateRequest dto)
        {
            try
            {
                using var content = CreateMultipartContent(dto, dto.NewFiles, dto.FilesToDelete);

                // URL: api/ProjectClassroom/{proClassId}/Update-content/{contentId}
                var resp = await _http.PutAsync($"api/ProjectClassroom/{proClassID}/Update-content/{contentId}", content);

                if (resp.IsSuccessStatusCode) return (true, null);

                var body = await resp.Content.ReadAsStringAsync();
                return (false, body);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }
    }
}