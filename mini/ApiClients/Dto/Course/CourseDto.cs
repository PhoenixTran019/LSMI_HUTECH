using System;
using System.Text.Json.Serialization;

namespace LmsMini.WebApp.ApiClients.Dto.Course
{
    // DTO cho phía Web App Client
    public class CourseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}