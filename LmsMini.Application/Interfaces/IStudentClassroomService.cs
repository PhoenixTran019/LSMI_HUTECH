using LmsMini.Application.DTOs.Common;
using LmsMini.Application.DTOs.Student;
using LmsMini.Application.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface IStudentClassroomService
    {

        Task<string?> JoinClassroomByCodeAsync(string inviteCode, string studentId);

        Task<List<ClassroomCardViewModel>> GetMyClassroomsAsync(string userId);

        Task<StudentLessonViewDto?> GetLessonDetailAsync(string classroomId, string lessonId, string studentId);

        Task<FileDownloadInfo?> GetLessonFileForDownloadAsync(string classroomId, string lessonId, string fileId, string studentId, string webRootPath);
    }
}
