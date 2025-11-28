using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class CreateProjectContentDto
    {
        public string ProClassID { get; set; }

        public string? Title { get; set; }

        public string? ContentText { get; set; }

        public string? ContentType { get; set; }

        public DateTime? Deadline { get; set; }

        //LIST FILE UPLOAD
        public List<IFormFile> Files { get; set; }
    }
}
