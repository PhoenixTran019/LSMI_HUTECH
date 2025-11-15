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
        Task<string> CreateLessonWithFilesAsync(CreateLessonWithFilesDto dto, string staffId, string webRootPath);

        Task<string> CreateAssignmentWithFilesAsync (CreateAssigmentWithFilesDto dto, string TeacherId, string webRootPath);
    }
}
