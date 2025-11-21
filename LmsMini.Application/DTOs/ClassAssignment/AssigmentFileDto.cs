using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ClassAssignment
{
    //DTO for assignment file details
    public class AssigmentFileDto
    {
        public string? FileName { get; set; } // Name of the file
        public string? FilePath { get; set; } // Path to the file (URL or relative path)
        public string? FileType { get; set; } // MIME type of the file
    }
}
