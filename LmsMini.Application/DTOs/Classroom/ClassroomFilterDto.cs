using Azure.Core.Pipeline;
using Microsoft.Identity.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Application.DTOs.Classroom
{
    public class ClassroomFilterDto
    {
        public string? Keyword { get; set; }    //Find by ClassName
        public string? SubjectId { get; set; }  //Find by ClassSub
        public string? DepartmentId { get; set; } //Find by Department of the Subject
        public string? MainClassId { get; set; } //Find by MainClass
        public string? Course { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
