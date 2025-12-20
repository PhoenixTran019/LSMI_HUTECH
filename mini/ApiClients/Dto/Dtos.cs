    using System;
    using System.Text.Json.Serialization;
    using System.ComponentModel.DataAnnotations;

    namespace LmsMini.WebApp.ApiClients.Dto
    {
        // Cần thêm DTO Dropdown cơ bản nếu không dùng thư mục Dropdowns
        public class ClassDto
        {
            [JsonPropertyName("ClassId")]
            public string? ClassId { get; set; }
            [JsonPropertyName("ClassName")]
            public string? ClassName { get; set; }
        }

        public class DepartDto
        {
            [JsonPropertyName("DepartId")]
            public string? DepartId { get; set; }

            [JsonPropertyName("DepartName")]
            public string? DepartName { get; set; }
        }

        public class MajorDto
        {
            [JsonPropertyName("MajorId")]
            public string? MajorId { get; set; }
            [JsonPropertyName("MajorName")]
            public string? MajorName { get; set; }
        }

        // ================= SUBJECT =================
        public class SubjectDto
        {
            [JsonPropertyName("SubId")]
            public string? SubId { get; set; }

            [JsonPropertyName("SubName")]
            public string? SubName { get; set; }
        }
    }