// File: LmsMini.WebApp.ApiClients.Dto.ProjectClassroom/ProjectClassroomDtos.cs

using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.ProjectClassroom
{
    // DTO 1: Tạo lớp học dự án
    public class CreateProjectClassroomRequest
    {
        [Required(ErrorMessage = "Project ID là bắt buộc.")]
        public string? ProjectID { get; set; }

        [Required(ErrorMessage = "Tên lớp là bắt buộc.")]
        public string? ClassroomName { get; set; }

        // CreatorLecturerID sẽ được lấy từ token (không cần thiết trong form)
        public string? CreatorLecturerID { get; set; }
    }

    // DTO 2: Thêm giảng viên
    public class AddLecturerRequest
    {
        // Các trường này sẽ được gán từ form hoặc code-behind
        [Required(ErrorMessage = "ID lớp là bắt buộc.")]
        public string ProClassID { get; set; } = string.Empty;

        [Required(ErrorMessage = "ID giảng viên mới là bắt buộc.")]
        public string NewMemberLecturerID { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò là bắt buộc.")]
        public string RoleInClass { get; set; } = string.Empty; // Ví dụ: "Giảng viên hướng dẫn"

        public string? AssignID { get; set; } // Nếu có

        // LecturerID: ID của người thực hiện thao tác (sẽ được lấy từ token)
    }

    // DTO 3: Dashboard/Danh sách lớp học của Staff
    public class MyProjectClassroomItem
    {
        public string ProClassID { get; set; } = string.Empty;
        public string? ClassroomName { get; set; }
        public string? ProjectTitle { get; set; }
        public string? LecturerName { get; set; }
    }
}