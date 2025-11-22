using LmsMini.Application.DTOs.Project;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface IProjectService
    {
        Task<string> CreateProjectAsync (CreateProjectDto dto, string staffId);
    }
}
