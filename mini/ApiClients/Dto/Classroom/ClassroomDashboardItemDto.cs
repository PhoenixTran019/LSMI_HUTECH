using System.Text.Json.Serialization;

namespace LmsMini.WebApp.ApiClients.Dto
{
    public class ClassroomDashboardItemDto
    {
        [JsonPropertyName("ClassroomId")]
        public string ClassroomId { get; set; } = "";

        [JsonPropertyName("ClassName")]
        public string ClassName { get; set; } = "";

        [JsonPropertyName("ClassSub")]
        public string ClassSub { get; set; } = "";

        [JsonPropertyName("MainClassName")]
        public string? MainClassName { get; set; }

        [JsonPropertyName("Course")]
        public string? Course { get; set; }

        [JsonPropertyName("LecturerName")]
        public string? LecturerName { get; set; }

        [JsonPropertyName("ClassStatus")]
        public string? ClassStatus { get; set; }

        [JsonPropertyName("InviteCode")]
        public string? InviteCode { get; set; }
    }
}
