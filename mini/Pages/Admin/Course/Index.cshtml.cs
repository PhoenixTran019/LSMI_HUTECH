using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.Course;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

namespace LmsMini.WebApp.Pages.Admin.Course
{
    [Authorize(Roles = "Staff,Admin")]
    public class CourseIndexModel : PageModel
    {
        private readonly ICourseApiClient _courseApi;

        public IList<CourseDto> Courses { get; set; } = new List<CourseDto>();

        // Dùng để hiển thị chi tiết một khóa học (nếu có id trong query string)
        public CourseDto? SelectedCourseDetail { get; set; }

        public CourseIndexModel(ICourseApiClient courseApi)
        {
            _courseApi = courseApi;
        }

        public async Task OnGetAsync(Guid? id)
        {
            // 1. Luôn tải danh sách khóa học
            Courses = await _courseApi.ListCoursesAsync();

            // 2. Nếu có ID, tải chi tiết khóa học đó
            if (id.HasValue)
            {
                var (detail, error) = await _courseApi.GetCourseByIdAsync(id.Value);
                if (detail != null)
                {
                    SelectedCourseDetail = detail;
                }
                // Nếu có lỗi, bạn có thể thêm logic hiển thị lỗi vào TempData
            }
        }
    }
}