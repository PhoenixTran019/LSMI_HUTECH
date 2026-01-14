using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs
{
    public class CreateStudentDto
    {
        public string? StudentID { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? DOB { get; set; }
        public string? Gender { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? ClassID { get; set; }
        public string? DepartID { get; set; }
        public string? StuMajor { get; set; }
        public DateOnly? EnrollmentDate { get; set; }
        public string? Password { get; set; }
    }
}
