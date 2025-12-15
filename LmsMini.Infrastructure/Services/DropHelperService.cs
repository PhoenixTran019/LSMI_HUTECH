using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LmsMini.Application.DTOs.DropHelper;
using LmsMini.Application.Interfaces;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace LmsMini.Infrastructure.Services
{
    public class DropHelperService : IDropHeplerService
    {

        private readonly LmsDbContext _context;

        public DropHelperService(LmsDbContext context)
        {
            _context = context;
        }

        public async Task<List<GroupProjectDropDto>> GetGroupsForLecturerAsync(string lecturerId)
        {
            var groups = await (
                from assign in _context.ProjectAssigns
                join mem in _context.ProjectClassMems
                    on assign.AssignId equals mem.AssignId
                where mem.LecturerId == lecturerId
                    && mem.RoleInClass == "Lecturer"
                select new GroupProjectDropDto
                {
                    AssignID = assign.AssignId,
                    GroupName = assign.GroupName
                }
            ).Distinct().ToListAsync();
            
            return groups;
        }

        public async Task<List<ClassSubjectDropDto>> GetClassSubjectDropDownAsync()
        {
            var ClassSub = await _context.Subjects
                .Select(s => new ClassSubjectDropDto
                {
                    SubId = s.SubId,
                    SubName = s.SubName
                })
                .ToListAsync();

            return ClassSub;
        }

        public async Task<List<MainClassDropDto>> GetMainClassDropDownAsync()
        {
            var mainClass = await _context.Classes
                .Select(m => new MainClassDropDto
                {
                    ClassId = m.ClassId,
                    ClassName = m.ClassName
                })
                .ToListAsync();

            return mainClass;
        }

        public async Task<List<StaffInforDropDto>> GetStaffInforDorpAsync()
        {
            var staffInfor = await _context.DepartmentStaffs
                .Select(m => new StaffInforDropDto
                {
                    StaffID = m.StaffId,
                    StaffFullName = m.LastName + " " + m.FirstName
                })
                .ToListAsync();

            return staffInfor;
        }
    }
}
