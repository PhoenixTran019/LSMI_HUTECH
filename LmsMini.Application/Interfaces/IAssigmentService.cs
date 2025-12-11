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

        Task<StaffAssignmentDetailDto?> StaffGetAssignmentDetailAysnc(string classroomId, string assigmentId);

        Task<bool> UpdateAssignmentAsync(string classroomId,string assignmentId, UpdateAssignmentDto dto, string staffId, string webRootPath);

        Task<bool> DeleteAssignmentAsync(string classroomId,string assignmentId, string staffId, string webRootPath);
    }
}
