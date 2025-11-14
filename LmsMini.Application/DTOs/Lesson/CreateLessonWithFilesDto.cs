using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;


namespace LmsMini.Application.DTOs.Lesson
{
    public class CreateLessonWithFilesDto
    {
        public string SubID { get; set; }
        public string DepartID { get; set; }
        public string ClassroomID { get; set; }
        public string SubjectName { get; set; } //Using to set Name for Folder
        public string Title { get; set; }
        public string Content { get; set; }

        public List<IFormFile> Files { get; set; } //File upload from form
    }
}
