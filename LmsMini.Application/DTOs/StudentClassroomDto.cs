using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs
{
    /// <summary>
    /// Thông tin 1 classroom (lớp môn học) mà sinh viên đang tham gia.
    /// </summary>
    public class StudentClassroomDto
    {
        public string ClassroomId { get; set; } = null!;
        public string ClassroomName { get; set; } = null!;

        public string SubjectId { get; set; } = null!;
        public string SubjectName { get; set; } = null!;
        public string SubjectCode { get; set; } = null!;

        public string? InviteCode { get; set; }
        public string? ClassStatus { get; set; }
    }
}
