namespace LmsMini.WebApp.ApiClients.Dto.Student
{
    public class AssignmentDto
    {
        public string AssignId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime Deadline { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}