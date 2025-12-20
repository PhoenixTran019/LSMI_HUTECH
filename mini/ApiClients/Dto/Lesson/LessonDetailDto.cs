using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace LmsMini.WebApp.ApiClients.Dto.Lesson
{
    public class LessonDetailDto
    {
        public string? LessonId { get; set; }

        [JsonPropertyName("classroomId")] // Map với property ClassroomID của Backend
        public string? ClassroomID { get; set; }

        public string? Title { get; set; }
        public string? Content { get; set; }

        [JsonPropertyName("createdAt")] // Map với CreateAt của Backend
        public DateTime? CreatedAt { get; set; }

        public List<LessonFileDto> Files { get; set; } = new();
    }

    public class LessonFileDto
    {
        // QUAN TRỌNG: Cần đảm bảo Backend trả về field này. 
        // Nếu Backend DTO chưa có FilesId, bạn cần thêm vào Backend trước.
        [JsonPropertyName("filesId")]
        public string? FilesId { get; set; }

        public string? FileName { get; set; }
        public string? FileType { get; set; }
        // FilePath thường không cần trả về cho Client vì lý do bảo mật, chỉ cần ID để tải.
    }
}