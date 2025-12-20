using LmsMini.WebApp.ApiClients;
using LmsMini.WebApp.ApiClients.Dto.ProjectClassroom;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LmsMini.WebApp.Pages.Project.Classroom
{
    // Cấp quyền cho Staff, Giảng viên và Admin
    [Authorize(Roles = "Staff,Lecturer,Admin")]
    public class ProjectClassroomIndexModel : PageModel
    {
        private readonly IProjectClassroomApiClient _projectClassroomApi;
        // Có thể cần IProjectApiClient để lấy danh sách Project ID cho dropdown

        public ProjectClassroomIndexModel(IProjectClassroomApiClient projectClassroomApi)
        {
            _projectClassroomApi = projectClassroomApi;
        }

        // --- Data Display ---
        public IList<MyProjectClassroomItem> MyClassrooms { get; set; } = new List<MyProjectClassroomItem>();

        // --- Forms ---
        [BindProperty]
        public CreateProjectClassroomRequest CreateInput { get; set; } = new();

        [BindProperty]
        public AddLecturerRequest AddLecturerInput { get; set; } = new();

        // --- Temp Data ---
        [TempData] public string? Message { get; set; }
        [TempData] public string? MessageType { get; set; }

        public async Task OnGetAsync()
        {
            // Tải danh sách các lớp học dự án mà Staff/Lecturer này đang quản lý
            MyClassrooms = await _projectClassroomApi.GetMyClassroomsAsync();

            // TODO: Load danh sách Project ID (dùng cho dropdown trong form CreateInput)
            // Giả định: List<SelectListItem> ProjectOptions
        }

        // ================= Xử lý TẠO LỚP HỌC DỰ ÁN =================
        public async Task<IActionResult> OnPostCreateClassroomAsync()
        {
            if (!ModelState.IsValid)
            {
                MessageType = "danger";
                Message = "Lỗi xác thực. Vui lòng kiểm tra lại thông tin.";
                TempData["ShowCreateModal"] = "true";
                await OnGetAsync();
                return Page();
            }

            var (isSuccess, error, proClassId) = await _projectClassroomApi.CreateProjectClassroomAsync(CreateInput);

            if (isSuccess)
            {
                MessageType = "success";
                Message = $"Tạo lớp học dự án thành công! ID: {proClassId}";
                return RedirectToPage();
            }

            MessageType = "danger";
            Message = $"Tạo lớp học thất bại: {error}";
            TempData["ShowCreateModal"] = "true";
            await OnGetAsync();
            return Page();
        }

        // ================= Xử lý THÊM GIẢNG VIÊN =================
        public async Task<IActionResult> OnPostAddLecturerAsync(string proClassID)
        {
            if (!ModelState.IsValid || string.IsNullOrEmpty(proClassID))
            {
                MessageType = "danger";
                Message = "Lỗi xác thực hoặc thiếu ID lớp dự án.";
                await OnGetAsync();
                return Page();
            }

            AddLecturerInput.ProClassID = proClassID;

            var (isSuccess, error) = await _projectClassroomApi.AddLecturerAsync(proClassID, AddLecturerInput);

            if (isSuccess)
            {
                MessageType = "success";
                Message = $"Thêm giảng viên {AddLecturerInput.NewMemberLecturerID} thành công!";
                return RedirectToPage();
            }

            MessageType = "danger";
            Message = $"Thêm giảng viên thất bại: {error}";
            await OnGetAsync();
            return Page();
        }
    }
}