namespace LmsMini.WebApp.ApiClients.Dto.Student
{
    public class LessonDto
    {
        public string LessonId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public DateTime? PostedDate { get; set; }
    }
}