using LmsMini.Application.DTOs.Project;
using LmsMini.Domain.Models;
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

        Task<bool> ApproveAsync(ProjectApprovalDto dto, string approverId);

    }
}
