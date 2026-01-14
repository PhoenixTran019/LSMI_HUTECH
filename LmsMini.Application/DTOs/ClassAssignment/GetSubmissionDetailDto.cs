using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ClassAssignment
{
    public class GetSubmissionDetailDto
    {
        public string? StudentID { get; set; }

        public string? FullName { get; set; }

        public string? AssignmentID { get; set; }

        public string? AssginTitle { get; set; }

        public DateTime? Deadline { get; set; }

        public DateTime? SubmitAt { get; set; }

        public string? SubmitType { get; set; }

        public double? Grade { get; set; }

        public string? FeedBack { get; set; }

        public List<AssigmentFileDto>? SubmissionFile { get; set; } = new List<AssigmentFileDto>();
    }
}
