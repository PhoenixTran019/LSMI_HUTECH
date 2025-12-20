// File: LmsMini.WebApp.ApiClients.Dto.ProjectClassroom/ProjectContentDtos.cs

using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.ProjectClassroom
{
    // DTO 4: Tạo nội dung/Bài tập dự án
    public class CreateProjectContentRequest
    {
        // ProClassID sẽ được lấy từ Route/Query String

        [Required]
        public string? Title { get; set; }

        public string? ContentText { get; set; }

        [Required]
        public string? ContentType { get; set; } // Ví dụ: "Lesson", "Assignment", "Announcement"

        public DateTime? Deadline { get; set; } // Chỉ áp dụng cho Assignment

        [Display(Name = "File đính kèm")]
        public List<IFormFile>? Files { get; set; } = new();
    }

    // DTO 5: Cập nhật nội dung/Bài tập dự án
    public class ProContentUpdateRequest
    {
        // ProContentID và ProClassID sẽ được lấy từ Route/Page Model

        public string? Title { get; set; }
        public string? ContentText { get; set; }
        public string? ContentType { get; set; }
        public DateTime? Deadline { get; set; }

        // File Management
        public List<string>? FilesToDelete { get; set; } // ID file nếu muốn xóa
        public List<IFormFile>? NewFiles { get; set; } // New file muốn upload
    }

    // DTO 6: Chi tiết nội dung/Bài tập (Staff view)
    // Dùng ProjectContentDetailDto từ tầng Application (cần ánh xạ các DTO liên quan)
    // Tuy nhiên, để đơn giản, ta sẽ chỉ tạo lại các DTO cần thiết cho API Client

    public class ProjectContentDetailDto
    {
        public string ProContentID { get; set; } = string.Empty;
        public string? Title { get; set; }
        public string? ContentText { get; set; }
        public string? PostedBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public string? ContentType { get; set; }
        public DateTime? Deadline { get; set; }
        // ... (cần các DTO khác cho Files, Submissions, v.v.)
    }
}