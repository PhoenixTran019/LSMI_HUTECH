// ApiClients/Dto/Student/AssignmentSummaryDto.cs

using System;

namespace LmsMini.WebApp.ApiClients.Dto.Student // <--- THÊM NAMESPACE NÀY
{
    public class AssignmentSummaryDto
    {
        public string AssignId { get; set; } = "";
        public string Title { get; set; } = "";
        public string ClassroomName { get; set; } = "";
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = "";
    }
}