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
            //Lectuer 
            var groups = await (
                from assign in _context.ProjectAssigns
                join mem in  _context.ProjectClassMems
                    on assign.AssignId equals mem.AssignId
                where mem.ProClassId == proClassId
                    && mem.LecturerId == lecturerId
                    && mem.RoleInClass == "Lecturer"
                select new GroupProjectDropDto
                {
                    AssignID = assign.AssignId,
                    GroupName = assign.GroupName
                }
                ).Distinct().ToListAsync();

            return groups;
        }
    }
}
