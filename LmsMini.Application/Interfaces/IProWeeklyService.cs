using LmsMini.Application.DTOs.ProjectClassroom;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface IProWeeklyService
    {
        Task<string> CreateMeetingAsync(string proClassId, CreateMeetingDto dto, string staffId);
    }
}
