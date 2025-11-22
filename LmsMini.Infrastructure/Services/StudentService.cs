using LmsMini.Domain.Models;
using LmsMini.Application.Interfaces;
using LmsMini.Application.DTOs;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LmsMini.Application.Common.Helpers;

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
        public async Task<bool> CreateStudentWithAccountAsync (CreateStudentDto dto, string staffID)
        {
            if (dto == null) throw new ArgumentNullException(nameof(dto));

            //Lấy ID thực từ dropdown "ID | Name"
            string departId = dto.DepartID?.Split(" | ")[0]?.Trim()
                ?? throw new ArgumentException("DepartID is required");

            string classId = dto.ClassID?.Split(" | ")[0]?.Trim()
                ?? throw new ArgumentException("ClassID is required");

            string majorId = dto.StuMajor?.Split(" | ")[0]?.Trim()
                ?? throw new ArgumentException("StuMajor is required");

            // Normalize to not differentiate between upper/lower case
            var normalizedStudentID = dto.StudentID.Trim().ToLower();

            //Check if StudentID or username already exists
            bool exists = await _context.Students.AnyAsync(s => s.StudentId.ToLower() == normalizedStudentID) ||
                          await _context.Users.AnyAsync(u => u.Username.ToLower() == normalizedStudentID);
            
            if (exists)
                return false;

            //Role Student
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Student");
            if (role == null)
                return false;

            //Create UUID v7 for UserId
            var userId = Uuidv7Generator.NewUuid7().ToString();

            //Create User and Student entities
            var user = new User
            {
                UserId = userId,
                Username = normalizedStudentID,
                PasswordHash = _jwtService.HashPassword(dto.StudentID),
                RoleId = role.RoleId,
                Status = "Active",
            };

            var student = new Student
            {
                StudentId = normalizedStudentID,
                UserId = userId,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Dob = dto.DOB,
                Gender = dto.Gender,
                PhoneNum = dto.PhoneNumber,
                DepartId = departId,
                ClassId = classId,
                StuMajor = majorId,
                EnrollmentDate = dto.EnrollmentDate,
            };

            //Write new log
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffID,
                DepartId = departId,
                Action = "Create new Student and their Account",
                TargetTable = "Student, Users",
                TargetId = normalizedStudentID,
                TargetName = $"{dto.FirstName} {dto.LastName}",
                Timestap = DateTime.UtcNow
            };

            //Add to DbContext and save changes
            _context.Users.Add(user);
            _context.Students.Add(student);
            _context.ActivityLogs.Add(log);

            //Save into database
            await _context.SaveChangesAsync();

            return true;

        }
    }
}
