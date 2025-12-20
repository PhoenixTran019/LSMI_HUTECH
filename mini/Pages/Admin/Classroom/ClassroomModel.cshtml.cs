using LmsMini.Application.DTOs.Classroom;
using LmsMini.WebApp.ApiClients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;

using ClassroomDashboardItemDto = LmsMini.WebApp.ApiClients.Dto.ClassroomDashboardItemDto;

[Authorize(Roles = "Staff,Admin,Lecturer")]
public class ClassroomModel : PageModel
{
    private readonly IClassroomApiClient _classroomApi;
    private readonly IStudentApiClient _studentApi;

    public List<ClassroomDashboardItemDto> Classrooms { get; set; } = new();

    [BindProperty(SupportsGet = true)]
    public ClassroomFilterDto Filter { get; set; } = new();

    [BindProperty]
    public CreateClassroomForm CreateForm { get; set; } = new();

    public List<SelectListItem> ClassOptions { get; set; } = new();
    public List<SelectListItem> CreateClassOptions { get; set; } = new();
    public List<SelectListItem> DepartmentOptions { get; set; } = new();

    // ✅ gợi ý tên môn (string)
    public List<SelectListItem> SubjectOptions { get; set; } = new();

    [TempData] public string? Message { get; set; }
    [TempData] public string? MessageType { get; set; }

    public ClassroomModel(IClassroomApiClient classroomApi, IStudentApiClient studentApi)
    {
        _classroomApi = classroomApi;
        _studentApi = studentApi;
    }

    public async Task OnGetAsync()
    {
        await LoadDropdowns();
        await LoadClassrooms();
    }

    public async Task<IActionResult> OnPostCreateAsync()
    {
        ModelState.Remove("Filter.Keyword");
        ModelState.Remove("Filter.DepartmentId");
        ModelState.Remove("Filter.MainClassId");

        if (!ModelState.IsValid)
        {
            await LoadDropdowns();
            await LoadClassrooms();
            TempData["ShowCreateClassroomModal"] = "true";
            return Page();
        }

        var dto = new CreateClassroomDto
        {
            ClassName = CreateForm.ClassName,
            ClassSub = CreateForm.ClassSub,   // ✅ tên môn (string)
            MainClass = CreateForm.MainClass,
            Description = CreateForm.Description,
            InviteCode = CreateForm.InviteCode,
            ClassStatus = CreateForm.ClassStatus
        };

        var result = await _classroomApi.CreateClassroomAsync(dto);

        Message = result.IsSuccess ? "Tạo lớp học thành công" : result.Error;
        MessageType = result.IsSuccess ? "success" : "danger";

        return RedirectToPage();
    }

    private async Task LoadDropdowns()
    {
        var classes = await _studentApi.GetClassesAsync();
        var depts = await _studentApi.GetDepartmentsAsync();

        ClassOptions = classes
            .Select(c => new SelectListItem(c.ClassName ?? "", c.ClassId?.ToString() ?? ""))
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToList();

        CreateClassOptions = ClassOptions;

        DepartmentOptions = depts
            .Select(d => new SelectListItem(d.DepartName ?? "", d.DepartId?.ToString() ?? ""))
            .Where(x => !string.IsNullOrWhiteSpace(x.Value))
            .ToList();

        // ✅ gợi ý môn từ dashboard-classrooms
        SubjectOptions = new();
        var subjectNames = await GetSubjectNameSuggestionsAsync();
        SubjectOptions.AddRange(subjectNames.Select(s => new SelectListItem(s, s)));
    }

    private async Task<List<string>> GetSubjectNameSuggestionsAsync()
    {
        try
        {
            var items = await _classroomApi.GetDashboardClassroomsAsync(new ClassroomFilterDto());
            return items.Select(x => x.ClassSub)
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Distinct()
                        .OrderBy(s => s)
                        .ToList();
        }
        catch
        {
            return new();
        }
    }

    private async Task LoadClassrooms()
    {
        Classrooms = await _classroomApi.GetDashboardClassroomsAsync(Filter);
    }
}

public class CreateClassroomForm
{
    [Required(ErrorMessage = "Tên lớp là bắt buộc")]
    public string ClassName { get; set; } = string.Empty;

    // ✅ Swagger: string (tên môn)
    [Required(ErrorMessage = "Môn học là bắt buộc")]
    public string ClassSub { get; set; } = string.Empty;

    [Required(ErrorMessage = "Lớp chính là bắt buộc")]
    public string MainClass { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mô tả là bắt buộc")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "InviteCode là bắt buộc")]
    public string InviteCode { get; set; } = string.Empty;

    public string ClassStatus { get; set; } = "InTime";
}
