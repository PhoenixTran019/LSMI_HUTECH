using LmsMini.Application.DTOs.DropHelper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
     public interface IDropHeplerService
    {
        Task<List<GroupProjectDropDto>> GetGroupsForLecturerAsync(string lecturerId);

        Task<List<ClassSubjectDropDto>> GetClassSubjectDropDownAsync();

        Task<List<MainClassDropDto>> GetMainClassDropDownAsync();
    }
}
