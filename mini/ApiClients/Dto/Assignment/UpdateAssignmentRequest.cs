using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.Assignment
{
    public class UpdateAssignmentRequest
    {
        [Display(Name = "Tiêu đề")]
        public string? Title { get; set; }

        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Hạn nộp")]
        public DateTime? Deadline { get; set; }

        [Display(Name = "Cho phép nộp muộn")]
        public string? HomeworkStatus { get; set; }

        [Display(Name = "File mới")]
        public List<IFormFile>? NewFiles { get; set; }

        // Danh sách ID/Tên File gốc để xóa (tùy thuộc API)
        public List<string>? RemoveFileId { get; set; }

        // Các thuộc tính dùng cho đường dẫn file (Nếu thay đổi)
        public string? ClassName { get; set; }
        public string? AssigmentName { get; set; }
    }
}