using System;
using System.Threading.Tasks;
using LmsMini.Application.Common.Helpers;
using LmsMini.Application.DTOs;
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
        /// Username của tài khoản sẽ = StudentID (để login / JWT claim dùng được).
        /// </summary>
        /// <param name="dto">Thông tin sinh viên từ DTO</param>
        /// <param name="staffId">StaffID của người tạo (TrainingManager/Admin)</param>
        /// <returns>
        /// true: tạo thành công;
        /// false: StudentID đã tồn tại hoặc chưa cấu hình Role "Student".
        /// </returns>
        public async Task<bool> CreateStudentWithAccountAsync(CreateStudentDto dto, string staffId)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            // -----------------------------------------------------------------
            // 1. Kiểm tra StudentID đã tồn tại chưa
            // -----------------------------------------------------------------
            var exists = await _context.Students
                .AnyAsync(s => s.StudentId == dto.StudentID);

            if (exists)
            {
                // StudentID đã tồn tại -> trả false để controller trả BadRequest
                return false;
            }

            // -----------------------------------------------------------------
            // 2. Lấy Role "Student"
            // -----------------------------------------------------------------
            var studentRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == "Student");

            if (studentRole == null)
            {
                // Chưa cấu hình Role "Student" trong bảng Roles
                return false;
            }

            // -----------------------------------------------------------------
            // 3. Tạo User mới (Username = StudentID)
            // -----------------------------------------------------------------
            var userId = Uuidv7Generator.NewUuid7().ToString();

            var user = new User
            {
                UserId = userId,
                // RẤT QUAN TRỌNG: Username = StudentID để đúng yêu cầu nhóm
                Username = dto.StudentID,
                PasswordHash = _jwtService.HashPassword(dto.Password),
                RoleId = studentRole.RoleId,
                Status = "Active" // bạn có thể đổi thành trạng thái khác nếu muốn
                // CreatedAt để DB tự set default GETDATE()
            };

            // -----------------------------------------------------------------
            // 4. Tạo Student mới, link với User vừa tạo
            // -----------------------------------------------------------------
            var student = new Student
            {
                StudentId = dto.StudentID,
                UserId = userId,
                DepartId = dto.DepartID,
                ClassId = dto.ClassID,
                StuMajor = dto.StuMajor,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PhoneNum = dto.PhoneNumber,
                // tạm để Emergency phone trống, nếu sau này FE có field thì map vào
                PhoneEmer = null,
                Mail = dto.Email,
                // Entity scaffold từ SQL thường là Dob (không phải DOB)
                Dob = dto.DOB,
                // Đảm bảo cột Gender đã được thêm vào bảng Students + Entity Student
                Gender = dto.Gender,
                Address = null,     // có thể cho FE nhập sau
                Image = null,       // avatar sau
                EnrollmentDate = dto.EnrollmentDate
            };

            // -----------------------------------------------------------------
            // 5. Dùng transaction để đảm bảo: hoặc tạo cả User + Student, hoặc rollback
            // -----------------------------------------------------------------
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                _context.Users.Add(user);
                _context.Students.Add(student);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw; // để bubble lên log lỗi ra ngoài (Swagger / console)
            }
        }
    }
}
