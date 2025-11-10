using LmsMini.Application.DTOs;
using LmsMini.Domain.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface IStudentService
    {
        Task<bool> CreateStudentWithAccountAsync(CreateStudentDto dto, string staffId);
    }
}
