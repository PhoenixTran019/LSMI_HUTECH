using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Classroom
{
    public class MemberInfoDto
    {
        public  string? MemberId { get; set; } // ID của bản ghi ClassroomMember

        public string? UserId { get; set; } // ID chung của người dùng (StudentId hoặc LecturerId)

        public string? FullName{ get; set; }


        public string? Email { get; set; }

        public string? RoleInClass { get; set; }
    }
}
