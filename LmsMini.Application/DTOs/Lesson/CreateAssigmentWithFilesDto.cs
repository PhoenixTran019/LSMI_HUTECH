using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LmsMini.Application.DTOs.Lesson
{
    public class CreateAssigmentWithFilesDto
    {
        public string ClassrooomID { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime Deadline { get; set; }
        public string HomeworkStatus { get; set; } //AllowLate, NoLate
        public List<IFormFile> Files { get; set; }
    }
}
