using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.ProjectClassroom
{
    public class CreateMeetingDto
    {
        public DateTime? MeetingDate { get; set; } 

        public string? ProLinkMeeting {  get; set; }

        public string? Notes { get; set; }
    }
}
