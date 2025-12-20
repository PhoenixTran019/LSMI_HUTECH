using System.Collections.Generic;
using System.Threading.Tasks;
using LmsMini.WebApp.ApiClients.Dto.Students;
namespace LmsMini.WebApp.ApiClients
{
    // ====================================================================
    // INTERFACE GỘP: Chứa tất cả các phương thức cần thiết cho Student
    // ====================================================================
    public interface IStudentApiClient
    {
        // --- Phương thức quản lý (cho Dropdown/Tạo mới Student) ---
        Task<List<ClassDto>> GetClassesAsync();
        Task<List<DepartDto>> GetDepartmentsAsync();
        Task<List<MajorDto>> GetMajorsAsync();
        Task<(bool IsSuccess, string? Error)> CreateStudentAsync(CreateStudentRequestDto dto);

        // --- Phương thức cho Student Portal (lấy thông tin cá nhân) ---
        Task<StudentProfileDto?> GetStudentProfileAsync(string studentId);
    }

    // ====================================================================
    // DTOs ĐƠN GIẢN (Cần thiết cho Interface này)
    // LƯU Ý: Nếu các DTO này trùng lặp với nơi khác, bạn nên di chuyển chúng.
    // ====================================================================

    public class StudentProfileDto
    {
        public string FirstName { get; set; } = "";
        public string LastName { get; set; } = "";
        public string Mail { get; set; } = ""; // Giả định thêm trường mail
    }

    // DTOs cho Dropdowns (Giả định nằm ở đây)
    public class ClassDto { public string ClassId { get; set; } = ""; public string ClassName { get; set; } = ""; }
    public class DepartDto { public string DepartId { get; set; } = ""; public string DepartName { get; set; } = ""; }
    public class MajorDto { public string MajorId { get; set; } = ""; public string MajorName { get; set; } = ""; }

    // DTO Request cho việc tạo mới
    // (Cần bổ sung các trường đầy đủ theo nhu cầu của bạn)
}