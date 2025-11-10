using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class Major
{
    public string MajorId { get; set; } = null!;

    public string? MajorName { get; set; }

    public string? MajorDescription { get; set; }

    public virtual ICollection<Class> Classes { get; set; } = new List<Class>();

    public virtual ICollection<DepartmentStaff> DepartmentStaffs { get; set; } = new List<DepartmentStaff>();

    public virtual ICollection<Internship> Internships { get; set; } = new List<Internship>();

    public virtual ICollection<Project> Projects { get; set; } = new List<Project>();

    public virtual ICollection<Specialization> Specializations { get; set; } = new List<Specialization>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();
}
