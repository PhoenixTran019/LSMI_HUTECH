using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class ProjectStudentSubmitDto
    {
        public string? StudentID { get; set; }

        public string? StudentName { get; set; }

        public DateTime? SubmitDate { get; set; }

        public double? Score { get; set; }

        public string? Feedback { get; set; }

        public List<FileDto>? Files { get; set; }
    }
}
