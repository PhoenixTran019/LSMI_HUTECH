using System;

namespace LmsMini.Application.DTOs
{
    /// <summary>
    /// Dùng cho API sinh viên join lớp đồ án.
    /// InviteCode hiện tại được hiểu là ProClassID đang được share cho sinh viên.
    /// </summary>
    public class JoinProjectClassroomDto
    {
        /// <summary>
        /// Mã mời của lớp đồ án (ở DB hiện tại = ProClassID).
        /// </summary>
        public string InviteCode { get; set; } = string.Empty;

        /// <summary>
        /// Mã sinh viên thực hiện thao tác join.
        /// FE lấy từ profile / token.
        /// </summary>
        public string StudentID { get; set; } = string.Empty;
    }
}
