using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LmsMini.Domain.Models;

namespace LmsMini.Application.Models
{
    public class ClassroomCardViewModel
    {
        public string ClassroomId { get; set; }
        public string ClassName { get; set; }
        public string ClassSub { get; set; }
        public string MainClassName { get; set; }
        public string Course { get; set; }
        public string LecturerName { get; set; }
        public string ClassStatus { get; set; }
        public string InviteCode { get; set; }
    }
}
