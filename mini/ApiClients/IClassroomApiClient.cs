using LmsMini.Application.DTOs.Classroom;
using LmsMini.WebApp.ApiClients.Dto;
using LmsMini.WebApp.ApiClients.Dto.Classroom;
using LmsMini.WebApp.ApiClients.Dto.Lesson;
using LmsMini.WebApp.ApiClients.Dto.Student;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace LmsMini.WebApp.ApiClients
{
    public interface IClassroomApiClient
    {
        // === Phương thức Quản lý/Dashboard ===
        Task<List<ClassroomDashboardItemDto>> GetDashboardClassroomsAsync(ClassroomFilterDto filter);
        Task<ClassroomOverviewDto> GetClassroomOverviewAsync(string classroomId);
        Task<(bool IsSuccess, string? Error)> CreateClassroomAsync(CreateClassroomDto dto);

        // === Phương thức Student Portal ===
        Task<List<ClassroomListDto>?> GetClassroomsForStudentAsync(string studentId);
        Task<int> GetClassroomsCountForStudent(string studentId);
        Task<ClassDetailsDto?> GetClassDetailsForStudent(string classroomId, string studentId);
        Task<(bool IsSuccess, string? Error, string? ClassroomId)> JoinClassroomAsync(string studentId, string inviteCode);

        // === Phương thức Lesson ===
        Task<LessonDetailDto?> GetLessonDetailAsync(string classroomId, string lessonId);
        Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadLessonFileAsync(string classroomId, string fileId);

        // === Phương thức Assignment (Student) ===
        Task<StudentAssignmentDetailDto?> GetStudentAssignmentDetailAsync(string classroomId, string assignmentId, string studentId);
        Task<(bool IsSuccess, string? Message)> SubmitAssignmentAsync(string classroomId, string assignmentId, List<BrowserFileDto> files);
        Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadAssignmentFileAsync(string classroomId, string fileId);
        Task<(Stream? FileStream, string? ContentType, string? FileName)> DownloadSubmissionFileAsync(string classroomId, string submitFileId);
    }
}