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
        Task<string> CreateProjectClassroomAsync(CreateProjectClassroomDto dto, string staffId);

        Task<List<MyClassroomDto>> GetMyClassroomsAsync(string staffId);

        Task AddMemberAsync(AddLecturerToProjectClassroomDto dto, string staffId);

        Task<ProjectClassroomDashboardDto> GetDashboardAsync(string proClassId);

        Task CreateProjectContentAsync(CreateProjectContentDto dto, string staffId);

        Task<ProjectContentDetailDto?> GetContentDetailAsync(string contentId, string proClassId);

        Task UpdateProjectContentAsync (ProContentUpdateDto dto, string staffId);

        Task DeleteProContentAsync(string proClassId, string contentId, string staffId);
    }
}
