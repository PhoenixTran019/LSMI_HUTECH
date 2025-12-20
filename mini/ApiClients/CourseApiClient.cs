using LmsMini.WebApp.ApiClients.Dto.Course;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace LmsMini.WebApp.ApiClients
{
    public class CourseApiClient : ICourseApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public CourseApiClient(HttpClient http)
        {
            _http = http;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // 1. LẤY CHI TIẾT KHÓA HỌC (Giả định endpoint GET api/Courses/{courseId} đã tồn tại)
        public async Task<(CourseDto? Course, string? Error)> GetCourseByIdAsync(Guid courseId)
        {
            try
            {
                var resp = await _http.GetAsync($"api/Courses/{courseId}");

                if (resp.IsSuccessStatusCode)
                {
                    var course = await resp.Content.ReadFromJsonAsync<CourseDto>(_jsonOptions);
                    return (course, null);
                }

                var errorBody = await resp.Content.ReadAsStringAsync();
                return (null, errorBody);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi kết nối hoặc Deserialize: {ex.Message}");
            }
        }

        // 2. LẤY DANH SÁCH KHÓA HỌC (Giả định endpoint GET api/Courses đã tồn tại)
        public async Task<List<CourseDto>> ListCoursesAsync()
        {
            try
            {
                var resp = await _http.GetAsync("api/Courses");
                resp.EnsureSuccessStatusCode();

                var courses = await resp.Content.ReadFromJsonAsync<List<CourseDto>>(_jsonOptions);
                return courses ?? new List<CourseDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API CLIENT ERROR] Lỗi khi lấy danh sách khóa học: {ex.Message}");
                return new List<CourseDto>();
            }
        }
    }
}