using LmsMini.Application.DTOs.ClassAssignment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface IAssigmentService
    {
        Task<string> CreateAssignmentWithFilesAsync(CreateAssigmentWithFilesDto dto, string teacherId, string webRootPath);

        Task<StaffAssignmentDetailDto?> StaffGetAssignmentDetailAysnc(string assigmentId);
    }
}
