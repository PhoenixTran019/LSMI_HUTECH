    using System.Text.Json.Serialization;

namespace LmsMini.WebApp.ApiClients.Dto.Students // ĐÃ SỬA NAMESPACE
{
    // DTO cho việc hiển thị danh sách SV
    public class StudentDto
    {
        [JsonPropertyName("StudentID")]
        public string? StudentId { get; set; }

        [JsonPropertyName("FullName")]
        public string? FullName { get; set; }

        [JsonPropertyName("Class")]
        public string? ClassName { get; set; }

        [JsonPropertyName("Major")]
        public string? MajorName { get; set; }

        [JsonPropertyName("Email")]
        public string? Email { get; set; }
    }
}