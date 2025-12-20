using LmsMini.Application.DTOs.Classroom;
using LmsMini.WebApp.ApiClients.Dto;
using LmsMini.WebApp.ApiClients.Dto.Classroom;
using LmsMini.WebApp.ApiClients.Dto.Lesson;
using LmsMini.WebApp.ApiClients.Dto.Student;
using Microsoft.AspNetCore.WebUtilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LmsMini.WebApp.ApiClients
{
    public class ClassroomApiClient : IClassroomApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public ClassroomApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("LmsApi");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        // ====================================================================
        // === 1. PHƯƠNG THỨC QUẢN LÝ / DASHBOARD ===
        // ====================================================================

        public async Task<List<ClassroomDashboardItemDto>> GetDashboardClassroomsAsync(ClassroomFilterDto filter)
        {
            var url = "api/Classroom/dashboard-classrooms";
            var query = new Dictionary<string, string?>();
            if (!string.IsNullOrWhiteSpace(filter.Keyword)) query["Keyword"] = filter.Keyword;
            if (!string.IsNullOrWhiteSpace(filter.MainClassId)) query["MainClassId"] = filter.MainClassId;
            url = QueryHelpers.AddQueryString(url, query);

            try
            {
                var resp = await _http.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadFromJsonAsync<List<ClassroomDashboardItemDto>>(_jsonOptions) ?? new();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ClassroomApiClient] GetDashboardClassroomsAsync error: {ex.Message}");
                return new();
            }
        }

        public async Task<ClassroomOverviewDto> GetClassroomOverviewAsync(string classroomId)
        {
            var resp = await _http.GetAsync($"api/Classroom/{classroomId}/Classroom-Dashboard-Detail");
            resp.EnsureSuccessStatusCode();
            return await resp.Content.ReadFromJsonAsync<ClassroomOverviewDto>(_jsonOptions)
                           ?? throw new InvalidOperationException("Overview is null");
        }

        public async Task<(bool IsSuccess, string? Error)> CreateClassroomAsync(CreateClassroomDto dto)
        {
            var resp = await _http.PostAsJsonAsync("api/Classroom/create-classroom", dto, _jsonOptions);
            if (resp.IsSuccessStatusCode) return (true, null);
            return (false, await resp.Content.ReadAsStringAsync());
        }

        // ====================================================================
        // === 2. STUDENT PORTAL ===
        // ====================================================================

        public async Task<List<ClassroomListDto>?> GetClassroomsForStudentAsync(string studentId)
        {
            try
            {
                var url = "api/Student/Classroom/My-Classrooms";
                var resp = await _http.GetAsync(url);
                if (!resp.IsSuccessStatusCode) return null;
                return await resp.Content.ReadFromJsonAsync<List<ClassroomListDto>>(_jsonOptions);
            }
            catch { return null; }
        }

        public async Task<(bool IsSuccess, string? Error, string? ClassroomId)> JoinClassroomAsync(string studentId, string inviteCode)
        {
            try
            {
                var url = "api/Student/Classroom/JoinClass-bycode";
                var jsonPayload = JsonSerializer.Serialize(inviteCode.Trim());
                using var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await _http.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseJson = await response.Content.ReadAsStringAsync();
                    using JsonDocument doc = JsonDocument.Parse(responseJson);
                    var root = doc.RootElement;

                    string? classroomId = null;
                    if (root.TryGetProperty("classroomId", out var idElement))
                        classroomId = idElement.GetString();
                    else if (root.TryGetProperty("ClassroomId", out idElement))
                        classroomId = idElement.GetString();

                    return (true, null, classroomId);
                }
                return (false, "Lỗi tham gia lớp học", null);
            }
            catch (Exception ex)
            {
                return (false, $"Lỗi kết nối: {ex.Message}", null);
            }
        }

        public async Task<int> GetClassroomsCountForStudent(string studentId)
        {
            var classes = await GetClassroomsForStudentAsync(studentId);
            return classes?.Count ?? 0;
        }

        public async Task<ClassDetailsDto?> GetClassDetailsForStudent(string classroomId, string studentId)
        {
            try
            {
                var url = $"api/Classroom/{classroomId}/Classroom-Dashboard-Detail";
                var response = await _http.GetAsync(url);

                if (response.IsSuccessStatusCode)
                {
                    var overview = await response.Content.ReadFromJsonAsync<ClassroomOverviewDto>(_jsonOptions);

                    if (overview != null)
                    {
                        return new ClassDetailsDto
                        {
                            ClassroomId = overview.ClassroomId,
                            ClassName = overview.ClassName,
                            LecturerName = overview.LecturerName,

                            Lessons = overview.Lessons?.Select(l => new LessonListItemDto
                            {
                                LessonId = l.LessonId,
                                Title = l.Title,
                                PostedDate = l.CreateAt ?? DateTime.Now
                            }).ToList() ?? new(),

                            Assignments = overview.Assignments?.Select(a => new AssignmentListItemDto
                            {
                                AssignId = a.AssignId,
                                Title = a.Title,
                                Deadline = a.Deadline ?? DateTime.MinValue,
                                Status = a.DeadlineStatus == "Valid" ? "Còn hạn" : "Quá hạn"
                            }).ToList() ?? new()
                        };
                    }
                }
                return await GetBasicInfoFallback(classroomId);
            }
            catch { return null; }
        }

        private async Task<ClassDetailsDto?> GetBasicInfoFallback(string classroomId)
        {
            try
            {
                var resp = await _http.GetAsync("api/Student/Classroom/My-Classrooms");
                if (resp.IsSuccessStatusCode)
                {
                    var list = await resp.Content.ReadFromJsonAsync<List<ClassroomListDto>>(_jsonOptions);
                    var item = list?.FirstOrDefault(x => x.ClassroomId == classroomId);
                    if (item != null)
                    {
                        return new ClassDetailsDto
                        {
                            ClassroomId = item.ClassroomId,
                            ClassName = item.ClassName,
                            LecturerName = item.LecturerName
                        };
                    }
                }
            }
            catch { }
            return null;
        }

        // ====================================================================
        // === 3. LESSON ===
        // ====================================================================

        public async Task<LessonDetailDto?> GetLessonDetailAsync(string classroomId, string lessonId)
        {
            try
            {
                // URL: api/Classroom/{classroomId}/Lessons/{lessonId}/detail
                // (Dựa trên [HttpGet("{lessonId}/detail")] trong LessonController)
                var url = $"api/Classroom/{classroomId}/Lessons/{lessonId}/detail";

                var response = await _http.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    // Log lỗi nếu cần
                    return null;
                }

                return await response.Content.ReadFromJsonAsync<LessonDetailDto>(_jsonOptions);
            }
            catch
            {
                return null;
            }
        }

        public async Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadLessonFileAsync(string classroomId, string filesId)
        {
            try
            {
                // URL: api/Classroom/{classroomId}/Lessons/files/{filesId}/download
                // (Dựa trên LessonFilesController)
                var url = $"api/Classroom/{classroomId}/Lessons/files/{filesId}/download";

                var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);

                if (!response.IsSuccessStatusCode) return (null, null, null);

                var stream = await response.Content.ReadAsStreamAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";

                string? fileName = null;
                if (response.Content.Headers.ContentDisposition != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileNameStar
                               ?? response.Content.Headers.ContentDisposition.FileName;
                }

                return (stream, contentType, fileName);
            }
            catch
            {
                return (null, null, null);
            }
        }

        // ====================================================================
        // === 4. STUDENT ASSIGNMENT ===
        // ====================================================================

        public async Task<StudentAssignmentDetailDto?> GetStudentAssignmentDetailAsync(string classroomId, string assignmentId, string studentId)
        {
            try
            {
                var url = $"api/Classroom/{classroomId}/Student/Assignments/{assignmentId}/detail";
                var response = await _http.GetAsync(url);
                if (!response.IsSuccessStatusCode) return null;
                return await response.Content.ReadFromJsonAsync<StudentAssignmentDetailDto>(_jsonOptions);
            }
            catch { return null; }
        }

        public async Task<(bool IsSuccess, string? Message)> SubmitAssignmentAsync(string classroomId, string assignmentId, List<BrowserFileDto> files)
        {
            try
            {
                var url = $"api/Classroom/{classroomId}/Student/Assignments/{assignmentId}/submit";
                using var content = new MultipartFormDataContent();

                content.Add(new StringContent(""), "SubmitType");

                foreach (var file in files)
                {
                    if (file.Content != null)
                    {
                        var fileContent = new StreamContent(file.Content);
                        fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                        content.Add(fileContent, "Files", file.FileName);
                    }
                }

                var response = await _http.PostAsync(url, content);
                if (response.IsSuccessStatusCode) return (true, "Nộp bài thành công!");

                var errorBody = await response.Content.ReadAsStringAsync();
                return (false, $"Lỗi: {errorBody}");
            }
            catch (Exception ex) { return (false, $"Lỗi kết nối: {ex.Message}"); }
        }

        public async Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadAssignmentFileAsync(string classroomId, string fileId)
        {
            var url = $"api/Classroom/{classroomId}/Student/Assignments/files/{fileId}/download";
            return await DownloadFileInternal(url);
        }

        public async Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadSubmissionFileAsync(string classroomId, string submitFileId)
        {
            var url = $"api/Classroom/{classroomId}/Student/Assignments/submission-files/{submitFileId}/download";
            return await DownloadFileInternal(url);
        }

        // ====================================================================
        // === PRIVATE HELPERS ===
        // ====================================================================

        // Chỉ giữ lại MỘT định nghĩa duy nhất cho hàm này
        private async Task<(Stream?, string?, string?)> DownloadFileInternal(string url)
        {
            try
            {
                var response = await _http.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
                if (!response.IsSuccessStatusCode) return (null, null, null);

                var stream = await response.Content.ReadAsStreamAsync();
                var contentType = response.Content.Headers.ContentType?.MediaType ?? "application/octet-stream";
                string? fileName = null;

                if (response.Content.Headers.ContentDisposition != null)
                {
                    fileName = response.Content.Headers.ContentDisposition.FileNameStar
                               ?? response.Content.Headers.ContentDisposition.FileName;
                }
                return (stream, contentType, fileName);
            }
            catch { return (null, null, null); }
        }
    }
}