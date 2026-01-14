using LmsMini.Application.DTOs.Common;

namespace LmsMini.Application.Interfaces
{
    public interface ILessonFileService
    {
        Task<FileDownloadInfo?> GetLessonFileForDownloadAsync(
            string classroomId,
            string filesId,
            string userId,
            bool isStudent,
            string webRootPath);
    }
}
