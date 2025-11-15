using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.StudentClassroom
{
    public class StudentSubmitDto
    {
        public string StudentId { get; set; } // Student ID
        public string? FullName { get; set; } // Student Full Name (get from user profile)
        public bool IsSubmitted { get; set; } // Submission status
        public bool IsLate { get; set; } // Late submission status
        public DateTime? SubmittedAt { get; set; } // Submission timestamp
    }
}
