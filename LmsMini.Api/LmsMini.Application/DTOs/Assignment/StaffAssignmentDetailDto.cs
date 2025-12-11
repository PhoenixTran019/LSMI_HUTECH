using LmsMini.Application.DTOs.StudentClassroom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ClassAssignment
{
    public class StaffAssignmentDetailDto
    {
        public string AssignID { get; set; }   // Assignment ID
        public string? Title { get; set; }        // Assignment Title
        public string CreatedBy { get; set; }    // Created By (Staff ID)
        public string? Description { get; set; }  // Assignment Description
        public DateTime? Deadline { get; set; }   // Assignment Deadline
        public string? DeadlineStatus { get; set; } // Status of the deadline (e.g., Valid, Overdue)
        public string? HomeworkStatus { get; set; } // Homework submission status (e.g., AllowLate, NoLate)
        public List<AssigmentFileDto> Files { get; set; } //List of attached files

        public List<StudentSubmitDto> Submit {  get; set; }//List student and status submit

    }
}
