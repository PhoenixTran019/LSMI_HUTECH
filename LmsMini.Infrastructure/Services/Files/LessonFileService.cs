using LmsMini.Application.DTOs.Common;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace LmsMini.Infrastructure.Services.Files
{
    public class LessonFileService : ILessonFileService
    {
        private readonly LmsDbContext _context;

        public LessonFileService(LmsDbContext context)
        {
            _context = context;
        }

        public async Task<FileDownloadInfo?> GetLessonFileForDownloadAsync(
            string classroomId,
            string filesId,
            string userId,
            bool isStudent,
            string webRootPath)
        {
            var fileRec = await _context.LessonFiles
                .AsNoTracking()
                .Include(f => f.Lesson)
                .FirstOrDefaultAsync(f =>
                    f.FilesId == filesId
                    && f.Lesson != null
                    && f.Lesson.ClassroomId == classroomId);

            if (fileRec == null) return null;

            // Nếu là student thì check membership
            if (isStudent)
            {
                var studentId = await _context.Students
                    .Where(s => s.UserId == userId)
                    .Select(s => s.StudentId)
                    .FirstOrDefaultAsync();

                if (string.IsNullOrWhiteSpace(studentId))
                    return null;

                var isMember = await _context.ClassroomMembers
                    .AnyAsync(m => m.ClassroomId == classroomId && m.StudentId == studentId);

                if (!isMember) return null;
            }

            if (string.IsNullOrWhiteSpace(fileRec.FilePath)) return null;

            var physicalPath = Path.Combine(
                webRootPath,
                fileRec.FilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(physicalPath)) return null;

            return new FileDownloadInfo
            {
                PhysicalPath = physicalPath,
                ContentType = string.IsNullOrWhiteSpace(fileRec.FileType) ? "application/octet-stream" : fileRec.FileType!,
                DownloadName = string.IsNullOrWhiteSpace(fileRec.FileName) ? Path.GetFileName(physicalPath) : fileRec.FileName!
            };
        }
    }
}
