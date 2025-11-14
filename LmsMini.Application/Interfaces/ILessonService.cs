using LmsMini.Application.DTOs.Lesson;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface ILessonService
    {
        Task<bool> CreateLessonWithFilesAsync(CreateLessonWithFilesDto dto, string staffId, string webRootPath);
    }
}
