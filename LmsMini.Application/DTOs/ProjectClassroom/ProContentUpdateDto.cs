using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class ProContentUpdateDto
    {
        public string ProContentID {  get; set; }

        public string ProClassID {  get; set; }

        public string? Title { get; set; }

        public string? ContentText { get; set; }

        public string? ContentType { get; set; }

        public DateTime? Deadline { get; set; }

        public List<string>? FilesToDelete { get; set; } //ID file if want to delete

        public List<IFormFile>? NewFiles {  get; set; } // New file want to upload

    }
}
