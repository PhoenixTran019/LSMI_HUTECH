using LmsMini.WebApp.ApiClients.Dto.Lesson;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http; // Cần cho IFormFile
using System.Text.Json.Serialization;
using System.Net.Http.Headers;
using System.IO;

namespace LmsMini.WebApp.ApiClients
{
    public class LessonApiClient : ILessonApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public LessonApiClient(HttpClient http)
        {
            _http = http;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        // --- HÀM HỖ TRỢ CHUNG CHO POST/PUT MULTIPART ---
        private MultipartFormDataContent CreateMultipartContent(
            string classroomId,
            object dto,
            IFormFile[]? files,
            Dictionary<string, string>? additionalData = null)
        {
            var content = new MultipartFormDataContent();

            // 1. Thêm các trường dữ liệu text (Form Data)
            // Lấy các thuộc tính từ DTO và thêm vào content
            var properties = dto.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(dto)?.ToString();
                // Bỏ qua IFormFile[] và List<string> (Files, RemoveFileName)
                if (prop.PropertyType != typeof(IFormFile[]) &&
                    prop.PropertyType != typeof(List<string>) &&
                    !string.IsNullOrEmpty(value))
                {
                    content.Add(new StringContent(value), $"\"{prop.Name}\"");
                }
            }

            // 2. Thêm file
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileStreamContent = new StreamContent(file.OpenReadStream());
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        // Tên field phải khớp với tên trong DTO API (Ví dụ: "Files")
                        content.Add(fileStreamContent, "Files", file.FileName);
                    }
                }
            }

            // 3. Thêm các trường dữ liệu bổ sung (ví dụ: RemoveFileName)
            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    content.Add(new StringContent(kvp.Value), $"\"{kvp.Key}\"");
                }
            }

            return content;
        }

        // 1. TẠO BÀI HỌC
        public async Task<(bool IsSuccess, string? Error, string? LessonId)> CreateLessonAsync(
            string classroomId,
            CreateLessonRequest dto,
            string className)
        {
            try
            {
                // Thêm ClassName vào DTO
                dto.ClassName = className;

                // Tạo Multipart Content. Files field trong API DTO là "Files"
                using var content = CreateMultipartContent(
                    classroomId,
                    dto,
                    dto.Files);

                // URL: api/Classroom/{classroomId}/Lessons/create-lesson
                var resp = await _http.PostAsync($"api/Classroom/{classroomId}/Lessons/create-lesson", content);

                var body = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                {
                    // Giả định API trả về { LessonID: "..." }
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("lessonId", out var lessonIdElement))
                    {
                        return (true, null, lessonIdElement.GetString());
                    }
                    return (true, null, null);
                }

                return (false, body, null); // Trả về body lỗi từ API
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}", null);
            }
        }

        // 2. LẤY CHI TIẾT BÀI HỌC
        public async Task<(LessonDetailDto? Detail, string? Error)> GetLessonDetailAsync(string classroomId, string lessonId)
        {
            try
            {
                // URL: api/Classroom/{classroomId}/Lessons/{lessonId}/detail
                var resp = await _http.GetAsync($"api/Classroom/{classroomId}/Lessons/{lessonId}/detail");

                if (!resp.IsSuccessStatusCode)
                {
                    return (null, $"Lỗi HTTP {(int)resp.StatusCode}: {resp.ReasonPhrase}");
                }

                var detail = await resp.Content.ReadFromJsonAsync<LessonDetailDto>(_jsonOptions);
                return (detail, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi kết nối hoặc Deserialize: {ex.Message}");
            }
        }

        // 3. CẬP NHẬT BÀI HỌC
        public async Task<(bool IsSuccess, string? Error)> UpdateLessonAsync(
            string classroomId,
            string lessonId,
            UpdateLessonRequest dto)
        {
            try
            {
                var additionalData = new Dictionary<string, string>();

                // Thêm các tên file cần xóa vào dữ liệu form
                if (dto.RemoveFileName != null && dto.RemoveFileName.Any())
                {
                    // API controller mong đợi List<string> với tên tham số là RemoveFileName
                    // Chúng ta cần serialize nó hoặc gửi từng phần tử (Serialization đơn giản hơn)
                    var jsonList = JsonSerializer.Serialize(dto.RemoveFileName);
                    additionalData.Add("RemoveFileName", jsonList);
                }

                using var content = CreateMultipartContent(
                    classroomId,
                    dto,
                    dto.NewFiles,
                    additionalData);

                // URL: api/Classroom/{classroomId}/Lessons/{lessonId}/Update-Lesson
                var resp = await _http.PutAsync($"api/Classroom/{classroomId}/Lessons/{lessonId}/Update-Lesson", content);

                if (resp.IsSuccessStatusCode) return (true, null);

                var errorBody = await resp.Content.ReadAsStringAsync();
                return (false, errorBody);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }

        // 4. XÓA BÀI HỌC
        public async Task<(bool IsSuccess, string? Error)> DeleteLessonAsync(string classroomId, string lessonId)
        {
            try
            {
                // URL: api/Classroom/{classroomId}/Lessons/{lessonId}/Delete-Lesson
                var resp = await _http.DeleteAsync($"api/Classroom/{classroomId}/Lessons/{lessonId}/Delete-Lesson");

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