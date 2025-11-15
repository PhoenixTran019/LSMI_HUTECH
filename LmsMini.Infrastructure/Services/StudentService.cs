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
            //check if StudentID already exists
            if (await _context.Students.AnyAsync(s => s.StudentId == dto.StudentID) ||
               await _context.Users.AnyAsync(u => u.Username == dto.StudentID))
            {
                return false; // StudentID already exists
            }

            //find roleID Student
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Student");
            if (role == null)
            {
                return false;
            }

            //create UUIDv7 for UserID
            var userID = Uuidv7Generator.NewUuid7().ToString();

            //create UserAccount
            var user = new User
            {
                UserId = userID,
                Username = dto.StudentID,
                PasswordHash = _jwtService.HashPassword(dto.StudentID),
                RoleId = role.RoleId,
                Status = "Active"
            };

            //Create Student
            var student = new Student
            {
                StudentId = dto.StudentID,
                UserId = userID,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Dob = dto.DOB,
                PhoneNum = dto.PhoneNumber,
                Mail = dto.Email,
                DepartId = dto.DepartID,
                ClassId = dto.ClassID,
                StuMajor = dto.StuMajor,
                EnrollmentDate = DateOnly.FromDateTime(dto.EnrollmentDate)
            };

            //Write new activities log
            var log = new ActivityLog
            {
                LogId = Uuidv7Generator.NewUuid7().ToString(),
                StaffId = staffID,
                DepartId = dto.DepartID,
                Action = "Create New Student",
                TargetTable = "Student",
                TargetId = student.StudentId,
                TargetName = $"{student.LastName} {student.FirstName}",
                Timestap = DateTime.UtcNow
            };

            _context.Users.Add(user);
            _context.Students.Add(student);
            _context.ActivityLogs.Add(log);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}
