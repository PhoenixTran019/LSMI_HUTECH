using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public class User
{
    public string UserId { get; set; } = null!;

    public string? Username { get; set; }

    public string? PasswordHash { get; set; }

    public string? RoleId { get; set; }

    public string? Status { get; set; }

    public DateTime? CreateAt { get; set; }

    public virtual ICollection<DepartmentStaff> DepartmentStaffs { get; set; } = new List<DepartmentStaff>();

    public virtual Role? Role { get; set; }

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
