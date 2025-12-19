using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ClassAssignment
{
    public class GradeSubmissionDto
    {
        public string? StudentId { get; set; }
        public double? Grade { get; set; }
        public string? FeedBack { get; set; }
    }
}
