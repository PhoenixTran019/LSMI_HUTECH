using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Classroom
{
    public class CreateClassroomDto
    {
        public string ClassName { get; set; }
        public string ClassSub { get; set; } //SubjectID for the class

        public string? MainClass { get; set; } //ClassesID nullable for main class
        public string Description { get; set; }
        public string InviteCode { get; set; }
        public string ClassStatus { get; set; } //Active, Inactive
    }
}
