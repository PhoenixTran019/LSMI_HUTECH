using Microsoft.AspNetCore.Http;
using System;
using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.Assignment
{
    public class CreateAssignmentRequest
    {
        [Required(ErrorMessage = "Tiêu đề Bài tập là bắt buộc.")]
        [Display(Name = "Tiêu đề")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Mô tả Bài tập là bắt buộc.")]
        [Display(Name = "Mô tả")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hạn nộp là bắt buộc.")]
        [Display(Name = "Hạn nộp")]
        public DateTime Deadline { get; set; }

        [Required(ErrorMessage = "Trạng thái nộp là bắt buộc.")]
        [Display(Name = "Cho phép nộp muộn")]
        public string HomeworkStatus { get; set; } = "NoLate"; // Ví dụ: "AllowLate" hoặc "NoLate"

        [Display(Name = "File đính kèm")]
        public List<IFormFile>? Files { get; set; }

        // Thuộc tính ẩn, được gán trong Page Model
        public string ClassName { get; set; } = string.Empty;
    }
}