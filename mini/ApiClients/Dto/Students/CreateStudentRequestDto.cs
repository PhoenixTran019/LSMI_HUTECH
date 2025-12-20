using System;
using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.Students
{
    // DTO cho Form Tạo SV
    public class CreateStudentRequestDto
    {
        // Thêm [Required] vào các trường bắt buộc để kích hoạt validation
        [Required(ErrorMessage = "Mã SV là bắt buộc")]
        public string? StudentID { get; set; }

        [Required(ErrorMessage = "Họ là bắt buộc")]
        public string? FirstName { get; set; }

        [Required(ErrorMessage = "Tên là bắt buộc")]
        public string? LastName { get; set; }

        public DateOnly? DOB { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Lớp là bắt buộc")]
        public string? ClassID { get; set; }

        [Required(ErrorMessage = "Khoa/Viện là bắt buộc")]
        public string? DepartID { get; set; }

        [Required(ErrorMessage = "Chuyên ngành là bắt buộc")]
        public string? StuMajor { get; set; }

        public DateOnly? EnrollmentDate { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự")]
        public string? Password { get; set; }
    }
}