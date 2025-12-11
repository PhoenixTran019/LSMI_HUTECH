using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Classroom
{
    public class UpdateClassroomDto
    {
        //2 entity block affter 14 days
        public string? ClassName { get; set; }

        public string? Description { get; set; }

        //Status allway ready to edit
        public string? ClassStatus { get; set; }
    }
}
