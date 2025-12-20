using LmsMini.WebApp.ApiClients.Dto.Lesson;

namespace LmsMini.WebApp.ApiClients
{
    public interface ILessonApiClient
    {
        // 1. Tạo Bài học
        Task<(bool IsSuccess, string? Error, string? LessonId)> CreateLessonAsync(
            string classroomId,
            CreateLessonRequest dto,
            string className); // Cần className để xây dựng path

        // 2. Lấy Chi tiết Bài học
        Task<(LessonDetailDto? Detail, string? Error)> GetLessonDetailAsync(
            string classroomId,
            string lessonId);

        // 3. Cập nhật Bài học
        Task<(bool IsSuccess, string? Error)> UpdateLessonAsync(
            string classroomId,
            string lessonId,
            UpdateLessonRequest dto);

        // 4. Xóa Bài học
        Task<(bool IsSuccess, string? Error)> DeleteLessonAsync(
            string classroomId,
            string lessonId);
    }
}