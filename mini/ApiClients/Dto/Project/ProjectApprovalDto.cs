// File: LmsMini.WebApp.ApiClients.Dto.Project/ProjectApprovalDto.cs
using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.Project
{
    public class ProjectApprovalDto
    {
        [Required]
        public string? AssignID { get; set; }

        public string? LecturerID { get; set; }

        [Required]
        public string? Decicion { get; set; }

        public string? Comments { get; set; }
    }
}