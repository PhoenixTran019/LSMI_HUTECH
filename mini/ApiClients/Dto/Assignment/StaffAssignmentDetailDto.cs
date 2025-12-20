using System;
using System.Collections.Generic;

namespace LmsMini.WebApp.ApiClients.Dto.Assignment
{
    // DTO này cần tham chiếu đến DTO StudentSubmitDto nếu nó tồn tại
    // Giả định StudentSubmitDto nằm trong Dto.StudentClassroom
    // using LmsMini.WebApp.ApiClients.Dto.StudentClassroom; 

    public class StaffAssignmentDetailDto
    {
        public string AssignID { get; set; }
        public string? Title { get; set; }
        public string CreatedBy { get; set; }
        public string? Description { get; set; }
        public DateTime? Deadline { get; set; }
        public string? DeadlineStatus { get; set; }
        public string? HomeworkStatus { get; set; }
        public List<AssigmentFileDto> Files { get; set; } = new();

        // Giả định StudentSubmitDto đã được tạo ở đâu đó
        // public List<StudentSubmitDto> Submit { get; set; } = new();
    }
}