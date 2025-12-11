using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ClassAssignment
{
    public class UpdateAssignmentDto
    {
        public string? AssignmentID { get; set; }

        public string? ClasroomID { get; set; }

        //Tesxt update
        public string? Title { get; set; }

        public string? Description { get; set; }

        public DateTime? Deadline { get; set; }

        public string? HomeworkStatus { get; set; } //AllowLate, NoLate

        //If FE want to change where the file save
        public string? ClassName { get; set; } //using to set nane for folder classroom
        public string? AssigmentName { get; set; } //using to set nane for folder assignment

        //Original files to delete
        public List<string>? RemoveFileId { get; set; }

        //Add new files
        public List<IFormFile>? NewFiles { get; set; }


    }
}
