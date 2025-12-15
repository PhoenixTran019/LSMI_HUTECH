using LmsMini.Application.Common.Helpers;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Services.Students
{
    public class StudentClassroomService :IStudentClassroomService
    {
        private readonly LmsDbContext _context;

        public StudentClassroomService(LmsDbContext context)
        {
            _context = context;
        }


        //==========SERVICE TO USING INVITE CODE TO JOIN TO CLASSROOM=========
        public async Task<string?> JoinClassroomByCodeAsync(string inviteCode, string studentId)
        {
            //Search Classroom by inviteCode and Status Active
            var classroom = await _context.Classrooms
                .Where(c => c.InviteCode == inviteCode && c.ClassStatus == "Active")
                .FirstOrDefaultAsync();

            if (classroom == null)
            {
                return null;
            }

            //Check student is in class
            var existingMember = await _context.ClassroomMembers
                .AnyAsync(m => m.ClassroomId == classroom.ClassroomId && m.StudentId == studentId);

            if (existingMember)
            {
                //if in class return Classroom ID
                return classroom.ClassroomId;
            }

            //AddStudent in to classroom
            var newMember = new ClassroomMember
            {
                MemberId = Uuidv7Generator.NewUuid7().ToString(),
                ClassroomId = classroom.ClassroomId,
                StudentId = studentId,
                RoleInClass = "Student"
            };

            _context.ClassroomMembers.Add(newMember);

            await _context.SaveChangesAsync();

            return classroom.ClassroomId;

        }
    }
}
