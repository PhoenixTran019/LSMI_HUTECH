using System;
using System.ComponentModel.DataAnnotations;

namespace LmsMini.WebApp.ApiClients.Dto.Project
{
    public class CreateProjectDto
    {
        [Required(ErrorMessage = "Tiêu đề là bắt buộc.")]
        [Display(Name = "Tiêu đề Dự án")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "Chuyên ngành là bắt buộc.")]
        [Display(Name = "Chuyên ngành")]
        public string? ProMajor { get; set; }

        [Display(Name = "Khóa")]
        public string? Cohort { get; set; }

        [Required(ErrorMessage = "Mô tả là bắt buộc.")]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }

        [Display(Name = "Số SV Tối đa")]
        [Range(1, 10, ErrorMessage = "Số sinh viên tối đa phải từ 1 đến 10.")]
        public int? MaxStudents { get; set; }

        [Display(Name = "Ngày bắt đầu")]
        public DateTime? StartDate { get; set; }

        [Display(Name = "Ngày kết thúc")]
        public DateTime? EndDate { get; set; }
    }
}