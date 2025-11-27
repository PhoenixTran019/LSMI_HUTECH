using System;

namespace LmsMini.Application.DTOs
{
    /// <summary>
    /// Thông tin hồ sơ sinh viên trả về cho FE.
    /// </summary>
    public class StudentProfileDto
    {
        public string StudentId { get; set; } = null!;
        public string? UserId { get; set; }

        public string? ClassId { get; set; }
        public string? ClassName { get; set; }

        public string? DepartId { get; set; }
        public string? DepartName { get; set; }

        public string? StuMajor { get; set; }
        public string? MajorName { get; set; }

        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? Gender { get; set; }

        public string? PhoneNum { get; set; }
        public string? Mail { get; set; }

        public string? Address { get; set; }
        public string? Image { get; set; }
    }
}
