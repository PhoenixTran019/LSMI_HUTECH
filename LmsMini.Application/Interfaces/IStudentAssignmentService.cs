using LmsMini.Application.DTOs.Common;
using LmsMini.Application.DTOs.Student;
using Microsoft.AspNetCore.Http;

namespace LmsMini.Application.Interfaces
{
    public interface IStudentAssignmentService
    {
        Task<List<StudentAssignmentListItemDto>> GetAssignmentsAsync(string classroomId, string userId);

        Task<StudentAssignmentDetailDto?> GetAssignmentDetailAsync(string classroomId, string assignmentId, string userId);

        Task<FileDownloadInfo?> GetAssignmentFileForDownloadAsync(string classroomId, string fileId, string userId, string webRootPath);

        Task<StudentSubmitResultDto> SubmitAssignmentAsync(
            string classroomId,
            string assignmentId,
            string userId,
            string? submitType,
            List<IFormFile> files,
            string webRootPath);

        Task<FileDownloadInfo?> GetMySubmissionFileForDownloadAsync(string classroomId, string submitFileId, string userId, string webRootPath);
    }
}
