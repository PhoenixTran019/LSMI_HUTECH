using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.Interfaces
{
    public interface IStudentClassroomService
    {

        Task<string?> JoinClassroomByCodeAsync(string inviteCode, string studentId);
    }
}
