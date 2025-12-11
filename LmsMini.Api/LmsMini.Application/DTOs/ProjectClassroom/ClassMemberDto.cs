using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class ClassMemberDto
    {
        public string ProClassMemID {  get; set; }

        public string? AssignID { get; set; }

        public string? LecturerID { get; set; }

        public string? RoleInClass {  get; set; }
    }
}
