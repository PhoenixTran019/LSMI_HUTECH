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
        public string ClassrooomID { get; set; } //Classroom ID
        public string ClassName { get; set; } //using to set nane for folder classroom
        public string LessonTitle { get; set; } //Using to set naeme for folder lesson
        public string Content { get; set; } //Lesson content/description

        public List<IFormFile> Files { get; set; } //File upload from form
    }
}
