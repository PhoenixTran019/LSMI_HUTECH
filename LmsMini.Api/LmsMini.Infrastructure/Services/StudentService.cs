using System;
using System.Linq;
using System.Threading.Tasks;
using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs.Student;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LmsMini.Infrastructure.Services
{
    public class StudentService : IStudentService
    {
        private readonly LmsDbContext _context;
        private readonly IJwtService _jwtService;

        public StudentService(LmsDbContext context, IJwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }

        /// <summary>
        /// Tạo sinh viên mới + account đăng nhập tương ứng.
        /// Username của tài khoản sẽ = StudentID (đã chuẩn hoá) để login / JWT claim dùng được.
        /// </summary>
        /// <param name="dto">Thông tin sinh viên từ DTO</param>
        /// <param name="staffId">StaffID của người tạo (TrainingManager/Admin)</param>
        /// <returns>
        /// true: tạo thành công;
        /// false: StudentID / Username đã tồn tại hoặc chưa cấu hình Role "Student".
        /// </returns>
        public async Task<bool> CreateStudentWithAccountAsync(CreateStudentDto dto, string staffId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // -----------------------------------------------------------------
            // 1. Chuẩn hoá StudentID & kiểm tra hợp lệ
            // -----------------------------------------------------------------
            var normalizedStudentId = dto.StudentID?.Trim();
            if (string.IsNullOrWhiteSpace(normalizedStudentId))
                throw new ArgumentException("StudentID is required.", nameof(dto));

            // Dùng lowercase để check trùng nhưng vẫn có thể lưu đúng form nếu sau này cần
            var normalizedLower = normalizedStudentId.ToLower();

            // -----------------------------------------------------------------
            // 2. Kiểm tra StudentID / Username đã tồn tại chưa (case-insensitive)
            // -----------------------------------------------------------------
            var studentExists = await _context.Students
                .AnyAsync(s => s.StudentId.ToLower() == normalizedLower);

            var userExists = await _context.Users
                .AnyAsync(u => u.Username.ToLower() == normalizedLower);

            if (studentExists || userExists)
            {
                // StudentID hoặc Username đã tồn tại -> trả false để controller trả BadRequest
                return false;
            }

            // -----------------------------------------------------------------
            // 3. Lấy entity Department / Class / Major từ ID trong DTO
            // -----------------------------------------------------------------
            var departEntity = await _context.Departments
                .FirstOrDefaultAsync(d => d.DepartId == dto.DepartID);

            if (departEntity == null)
                throw new ArgumentException("Invalid Department.", nameof(dto.DepartID));

            var classEntity = await _context.Classes
                .FirstOrDefaultAsync(c => c.ClassId == dto.ClassID);

            if (classEntity == null)
                throw new ArgumentException("Invalid Class.", nameof(dto.ClassID));

            var majorEntity = await _context.Majors
                .FirstOrDefaultAsync(m => m.MajorId == dto.StuMajor);

            if (majorEntity == null)
                throw new ArgumentException("Invalid Major.", nameof(dto.StuMajor));

            // -----------------------------------------------------------------
            // 4. Lấy Role "Student"
            // -----------------------------------------------------------------
            var studentRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Student");

            if (studentRole == null)
            {
                // Chưa cấu hình Role "Student" trong bảng Roles
                return false;
            }

            // -----------------------------------------------------------------
            // 5. Tạo User mới (Username = StudentID chuẩn hoá)
            // -----------------------------------------------------------------
            var userId = Uuidv7Generator.NewUuid7().ToString();

            // Nếu DTO không truyền password, dùng StudentID làm password mặc định
            var rawPassword = string.IsNullOrWhiteSpace(dto.Password)
                ? normalizedStudentId
                : dto.Password;

            var passwordHash = _jwtService.HashPassword(rawPassword);

            var user = new User
            {
                UserId = userId,
                // Quan trọng: Username = StudentID (đã chuẩn hoá) theo quy ước nhóm
                Username = normalizedStudentId,
                PasswordHash = passwordHash,
                RoleId = studentRole.RoleId,
                Status = "Active"
                // CreatedAt có thể để DB tự set default GETDATE()
            };

            // -----------------------------------------------------------------
            // 6. Tạo Student mới, link với User vừa tạo
            // -----------------------------------------------------------------
            var student = new Student
            {
                StudentId = normalizedStudentId,
                UserId = userId,
                DepartId = departEntity.DepartId,
                ClassId = classEntity.ClassId,
                StuMajor = majorEntity.MajorId,

                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNum = dto.PhoneNumber,

                // Tạm để Emergency phone / Address / Image trống,
                // nếu sau này FE có field thì map thêm.
                PhoneEmer = null,
                Mail = dto.Email,
                Dob = dto.DOB,
                Gender = dto.Gender,
                Address = null,
                Image = null,

                EnrollmentDate = dto.EnrollmentDate
            };

            // -----------------------------------------------------------------
            // 7. Ghi ActivityLog: ai (StaffId) đã tạo student + account
            // -----------------------------------------------------------------
            var staffDepartId = await _context.StaffDeparts
                .Where(s => s.StaffId == staffId)
                .Select(s => s.DepartId)
                .FirstOrDefaultAsync();

            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffId,
                DepartId = staffDepartId,
                Action = "Create new Student and their Account",
                TargetTable = "Students, Users",
                TargetId = normalizedStudentId,
                TargetName = $"{dto.FirstName} {dto.LastName}",
                Timestap = DateTime.UtcNow
            };

            // -----------------------------------------------------------------
            // 8. Dùng transaction để đảm bảo: hoặc tạo cả User + Student + Log, hoặc rollback
            // -----------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Users.Add(user);
                _context.Students.Add(student);
                _context.ActivityLogs.Add(log);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                // Cho phép bubble lỗi để log ra ngoài (Swagger / console)
                throw;
            }
        }
    }
}
