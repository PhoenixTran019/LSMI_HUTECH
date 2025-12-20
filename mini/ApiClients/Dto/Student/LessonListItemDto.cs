// ApiClients/Dto/Student/LessonListItemDto.cs

using System;

namespace LmsMini.WebApp.ApiClients.Dto.Student // <--- THÊM NAMESPACE NÀY
{
    public class LessonListItemDto
    {
        public string LessonId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        // Để fix lỗi DateTime?, chúng ta dùng DateTime không null
        public DateTime PostedDate { get; set; }
    }
}