// ApiClients/Dto/Student/ClassroomListDto.cs

using System.Collections.Generic;

namespace LmsMini.WebApp.ApiClients.Dto.Student // <--- THÊM NAMESPACE NÀY
{
    public class ClassroomListDto
    {
        public string ClassroomId { get; set; } = "";
        public string ClassName { get; set; } = "";
        public string SubjectName { get; set; } = "";
        public string LecturerName { get; set; } = "";
        public int TotalAssignments { get; set; }
    }
}