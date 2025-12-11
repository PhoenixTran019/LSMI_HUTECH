using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class StaffDepart
{
    public string? StaffId { get; set; }

    public string? DepartId { get; set; }

    public virtual DepartmentStaff Staff { get; set; }

    public virtual Department Department { get; set; }
}


