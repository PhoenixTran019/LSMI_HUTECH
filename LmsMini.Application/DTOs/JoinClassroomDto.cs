namespace LmsMini.Application.DTOs
{
    /// <summary>
    /// Body khi sinh viên nhập mã mời để join Classroom (lớp môn học).
    /// FE chỉ cần gửi InviteCode, Student được lấy từ token.
    /// </summary>
    public class JoinClassroomDto
    {
        public string InviteCode { get; set; } = string.Empty;
    }
}
