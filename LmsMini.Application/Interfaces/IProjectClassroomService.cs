using LmsMini.Application.DTOs.ProjectClassroom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface IProjectClassroomService
    {
        Task<string> CreateProjectClassroomAsync(CreateProjectClassroomDto dto);
    }
}
