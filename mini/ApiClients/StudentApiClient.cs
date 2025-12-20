using System.Text.Json;
using System.Text.Json.Serialization;
using LmsMini.WebApp.ApiClients.Dto.Students;
namespace LmsMini.WebApp.ApiClients
{
    public class StudentApiClient : IStudentApiClient
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _jsonOptions;

        public StudentApiClient(IHttpClientFactory factory)
        {
            _http = factory.CreateClient("LmsApi");

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = null,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNameCaseInsensitive = true
            };

            // Nếu bạn cần DateOnly, hãy bỏ comment dòng dưới (và đảm bảo file Helper tồn tại)
            // _jsonOptions.Converters.Add(new DateOnlyJsonConverter());
        }

        // --- HÀM HỖ TRỢ CHUNG AN TOÀN (Lấy danh sách cho Dropdown) ---
        private async Task<List<T>> GetDropdownDataSafeAsync<T>(string path) where T : class
        {
            try
            {
                var resp = await _http.GetAsync(path);

                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[API WARNING] {(int)resp.StatusCode} - {path}");
                    return new List<T>();
                }

                var json = await resp.Content.ReadAsStringAsync();

                // Cần xử lý cả trường hợp API trả về Array trực tiếp hoặc Object Wrapper { data: [...] }
                var list = JsonSerializer.Deserialize<List<T>>(json, _jsonOptions);
                return list ?? new List<T>();

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API CLIENT ERROR] {path}: {ex.Message}");
                return new List<T>();
            }
        }


        // ====================================================================
        // TRIỂN KHAI CÁC PHƯƠNG THỨC TỪ INTERFACE
        // ====================================================================

        // --- 1. Dropdown / Quản lý ---
        public async Task<List<ClassDto>> GetClassesAsync() => await GetDropdownDataSafeAsync<ClassDto>("api/LectoStudent/classes");

        public async Task<List<DepartDto>> GetDepartmentsAsync() => await GetDropdownDataSafeAsync<DepartDto>("api/LectoStudent/departments");

        public async Task<List<MajorDto>> GetMajorsAsync() => await GetDropdownDataSafeAsync<MajorDto>("api/LectoStudent/majors");

        public async Task<(bool IsSuccess, string? Error)> CreateStudentAsync(CreateStudentRequestDto dto)
        {
            try
            {
                var resp = await _http.PostAsJsonAsync("api/LectoStudent/create-student", dto, _jsonOptions);

                if (resp.IsSuccessStatusCode) return (true, null);

                var err = await resp.Content.ReadAsStringAsync();
                return (false, err);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API CLIENT FATAL] Lỗi khi tạo sinh viên: {ex.Message}");
                return (false, ex.Message);
            }
        }

        // --- 2. Student Portal ---
        public async Task<StudentProfileDto?> GetStudentProfileAsync(string studentId)
        {
            try
            {
                // Giả định endpoint API: api/Student/{studentId}/profile
                var path = $"api/Student/{studentId}/profile";
                var resp = await _http.GetAsync(path);

                if (!resp.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[API WARNING] {(int)resp.StatusCode} - {path}");
                    return null;
                }

                // Dùng ReadFromJsonAsync để deserialize
                return await resp.Content.ReadFromJsonAsync<StudentProfileDto>(_jsonOptions);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"[API CLIENT ERROR] Lỗi tải profile: {ex.Message}");
                return null;
            }
        }
    }
}