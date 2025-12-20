using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.Lesson
{
    public class CreateLessonRequest
    {
        [Required(ErrorMessage = "Tiêu đề bài học là bắt buộc.")]
        [Display(Name = "Tiêu đề bài học")]
        public string LessonTitle { get; set; } = string.Empty;

        [Display(Name = "Nội dung")]
        public string? Content { get; set; }

        // ClassName và ClassroomID sẽ được truyền qua routing/input ẩn
        public string? ClassName { get; set; }

        [Display(Name = "File đính kèm")]
        public IFormFile[]? Files { get; set; }
    }
}