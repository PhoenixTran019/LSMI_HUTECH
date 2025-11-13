using LmsMini.Application.DTOs.Classroom;
using LmsMini.Application.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface IClassroomService
    {
        

        Task<bool> CreateClassroomAsync (CreateClassroomDto dto, string staffId);

        Task<List<ClassroomCardViewModel>> GetDashboardClassroomsAsync(ClassroomFilterDto filter, List<string>? allowedDepartIds);
    }
}
