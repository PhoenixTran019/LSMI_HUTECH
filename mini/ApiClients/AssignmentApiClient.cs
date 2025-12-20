// LmsMini.WebApp.ApiClients/AssignmentApiClient.cs (ĐÃ SỬA LỖI CUỐI CÙNG)

using LmsMini.WebApp.ApiClients.Dto.Assignment;
using LmsMini.WebApp.ApiClients.Dto.Student;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

// Giả định AssignmentDetailDto, StaffAssignmentDetailDto, AssignmentSummaryDto (DTOs từ các phản hồi trước)
// nằm trong LmsMini.WebApp.ApiClients.Dto.Assignment hoặc đã được định nghĩa.


namespace LmsMini.WebApp.ApiClients
{
    public class AssignmentApiClient : IAssignmentApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public AssignmentApiClient(HttpClient http)
        {
            _http = http;
            _jsonOptions = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // --- HÀM HỖ TRỢ CHUNG CHO POST/PUT MULTIPART ---
        private MultipartFormDataContent CreateMultipartContent(
            object dto,
            List<IFormFile>? files,
            Dictionary<string, string>? additionalData = null)
        {
            var content = new MultipartFormDataContent();

            // 1. Thêm các trường dữ liệu text (Form Data)
            var properties = dto.GetType().GetProperties();
            foreach (var prop in properties)
            {
                var propType = prop.PropertyType;

                // Bỏ qua các thuộc tính là File List hoặc String List
                if (propType == typeof(List<IFormFile>) || propType == typeof(List<string>)) continue;

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

            // 2. Thêm file
            if (files != null)
            {
                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var fileStreamContent = new StreamContent(file.OpenReadStream());
                        fileStreamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                        // Tên field phải khớp với tên trong DTO API (Ví dụ: "Files" hoặc "NewFiles")
                        content.Add(fileStreamContent, "Files", file.FileName);
                    }
                }
            }

            // 3. Thêm các trường dữ liệu bổ sung (ví dụ: RemoveFileId)
            if (additionalData != null)
            {
                foreach (var kvp in additionalData)
                {
                    content.Add(new StringContent(kvp.Value), $"\"{kvp.Key}\"");
                }
            }

            return content;
        }


        // 1. TẠO BÀI TẬP
        public async Task<(bool IsSuccess, string? Error, string? AssignmentId)> CreateAssignmentAsync(
            string classroomId,
            CreateAssignmentRequest dto)
        {
            try
            {
                // Sử dụng 'Files' cho file list trong CreateAssignmentRequest
                using var content = CreateMultipartContent(dto, dto.Files);

                // URL: api/{classroomId}/Lessons/create-assignment
                var resp = await _http.PostAsync($"api/{classroomId}/Lessons/create-assignment", content);

                var body = await resp.Content.ReadAsStringAsync();

                if (resp.IsSuccessStatusCode)
                {
                    // Giả định API trả về { AssignmentID: "..." }
                    using var doc = JsonDocument.Parse(body);
                    if (doc.RootElement.TryGetProperty("assignmentID", out var assignIdElement))
                    {
                        return (true, null, assignIdElement.GetString());
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

        // 2. LẤY CHI TIẾT BÀI TẬP (Staff)
        public async Task<(StaffAssignmentDetailDto? Detail, string? Error)> StaffGetAssignmentDetailAsync(
            string classroomId,
            string assignmentId)
        {
            try
            {
                // URL: api/{classroomId}/Lessons/{assignmentId}/staff-assignment-detail
                var resp = await _http.GetAsync($"api/{classroomId}/Lessons/{assignmentId}/staff-assignment-detail");

                if (!resp.IsSuccessStatusCode)
                {
                    return (null, $"Lỗi HTTP {(int)resp.StatusCode}: {resp.ReasonPhrase}");
                }

                var detail = await resp.Content.ReadFromJsonAsync<StaffAssignmentDetailDto>(_jsonOptions);
                return (detail, null);
            }
            catch (Exception ex)
            {
                return (null, $"Lỗi kết nối hoặc Deserialize: {ex.Message}");
            }
        }

        // *******************************************************************
        // PHƯƠNG THỨC 1: GetPendingAssignmentsCountForStudent (Đã thêm)
        // *******************************************************************
        public async Task<int> GetPendingAssignmentsCountForStudent(string studentId)
        {
            try
            {
                // URL API giả định: api/Assignment/student/{studentId}/pending-count
                var url = $"api/Assignment/student/{studentId}/pending-count";

                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return 0;
                }

                var count = await response.Content.ReadFromJsonAsync<int>(_jsonOptions);
                return count;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching pending assignments count: {ex.Message}");
                return 0;
            }
        }

        // *******************************************************************
        // PHƯƠNG THỨC 2: GetAllAssignmentsForStudent (PHƯƠNG THỨC CẦN THIẾT)
        // *******************************************************************
        /// <summary>
        /// Lấy tất cả bài tập cho sinh viên từ mọi lớp học.
        /// </summary>
        public async Task<List<AssignmentSummaryDto>?> GetAllAssignmentsForStudent(string studentId) // <--- THÊM DẤU '?' VÀO ĐÂY
        {
            try
            {
                // URL API giả định: api/Assignment/student/{studentId}/all
                var url = $"api/Assignment/student/{studentId}/all";

                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                // Giả định AssignmentSummaryDto là DTO list cho trang tổng quan
                var assignments = await response.Content.ReadFromJsonAsync<List<AssignmentSummaryDto>>(_jsonOptions);
                return assignments;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching all assignments for student: {ex.Message}");
                return null;
            }
        }


        // 5. LẤY CHI TIẾT BÀI TẬP (Student)
        public async Task<AssignmentDetailDto?> GetAssignmentDetailsForStudent(string assignmentId, string studentId)
        {
            try
            {
                // Giả định URL API: api/Assignment/{assignmentId}/student/{studentId}/details
                var url = $"api/Assignment/{assignmentId}/student/{studentId}/details";

                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                var detail = await response.Content.ReadFromJsonAsync<AssignmentDetailDto>(_jsonOptions);
                return detail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error fetching student assignment details: {ex.Message}");
                return null;
            }
        }


        // 3. CẬP NHẬT BÀI TẬP
        public async Task<(bool IsSuccess, string? Error)> UpdateAssignmentAsync(
            string classroomId,
            string assignmentId,
            UpdateAssignmentRequest dto)
        {
            try
            {
                var additionalData = new Dictionary<string, string>();

                // Xử lý các file cần xóa (Nếu API nhận List<string> qua form data)
                if (dto.RemoveFileId != null && dto.RemoveFileId.Any())
                {
                    // Gửi danh sách các ID/Tên file cần xóa (Giả định API nhận JSON list qua form)
                    var jsonList = JsonSerializer.Serialize(dto.RemoveFileId);
                    additionalData.Add("RemoveFileId", jsonList);
                }

                // Sử dụng 'NewFiles' cho file list
                using var content = CreateMultipartContent(dto, dto.NewFiles, additionalData);

                // URL: api/{classroomId}/Lessons/update-assignment/{assignmentId}
                var resp = await _http.PutAsync($"api/{classroomId}/Lessons/update-assignment/{assignmentId}", content);

                if (resp.IsSuccessStatusCode) return (true, null);

                var errorBody = await resp.Content.ReadAsStringAsync();
                return (false, errorBody);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}");
            }
        }


        // 4. XÓA BÀI TẬP
        public async Task<(bool IsSuccess, string? Error)> DeleteAssignmentAsync(
            string classroomId,
            string assignmentId)
        {
            try
            {
                // URL: api/{classroomId}/Lessons/delete-assignment/{assignmentId}
                var resp = await _http.DeleteAsync($"api/{classroomId}/Lessons/delete-assignment/{assignmentId}");

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