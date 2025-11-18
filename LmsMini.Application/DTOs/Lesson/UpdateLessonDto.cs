using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Lesson
{
    public class UpdateLessonDto
    {
        public string? Title { get; set; } //If null nonchange

        public string? Content { get; set; } //If null no change

        public string? Classname { get; set; } //FE sent Classname to specify file save path

        public string? LessonName { get; set; } //FE sent  LessonName(new) to 

        public IFormFile[]? NewFiles { get; set; } //New file to add

        public List<string>? RemoveFileName { get; set; } //List File to delete
    }
}
