using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class AddLecturerToProjectClassroomDto
    {
        public string ProClassID {  get; set; } //ID Project Classroom

        public string LecturerID { get; set; } //Additional person (must be a Lecturer in the class)

        public string NewMemberLecturerID { get; set; }

        public string RoleInClass {  get; set; }

        public string?AssignID { get; set; } //If have
    }
}
