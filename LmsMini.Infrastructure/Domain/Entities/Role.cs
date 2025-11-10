using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class Role
{
    public string RoleId { get; set; } = null!;

    public string? RoleName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<DepartmentStaff> DepartmentStaffs { get; set; } = new List<DepartmentStaff>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
