using LmsMini.WebApp.ApiClients.Dto.Student;

public class ClassDetailsDto
{
    public string ClassroomId { get; set; } = string.Empty;
    public string ClassName { get; set; } = string.Empty;
    public string LecturerName { get; set; } = string.Empty;
    // Sử dụng đúng tên Class mà Razor Page đang gọi
    public List<LessonListItemDto> Lessons { get; set; } = new();
    public List<AssignmentListItemDto> Assignments { get; set; } = new();
}