using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.Lesson
{
    public class UpdateLessonRequest
    {
        [Display(Name = "Tiêu đề")]
        public string? Title { get; set; }

        [Display(Name = "Nội dung")]
        public string? Content { get; set; }

        // Các thuộc tính sau dùng để hỗ trợ API Client:
        public string? Classname { get; set; } // Tên lớp (cho đường dẫn lưu file)
        public string? LessonName { get; set; } // Tên bài học mới (cho đường dẫn lưu file)

        [Display(Name = "File mới")]
        public IFormFile[]? NewFiles { get; set; }

        // Danh sách tên file gốc để xóa
        public List<string>? RemoveFileName { get; set; }
    }
}