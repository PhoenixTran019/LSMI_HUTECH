namespace LmsMini.Application.DTOs.Student
{
    /// <summary>
    /// Dùng cho Student tự cập nhật thông tin liên lạc.
    /// Chỉ cho phép sửa các field mềm, không sửa mã số / lớp / khoa.
    /// </summary>
    public class UpdateStudentProfileDto
    {
        public string? PhoneNum { get; set; }
        public string? Mail { get; set; }
        public string? Address { get; set; }
        public string? Image { get; set; }
    }
}
