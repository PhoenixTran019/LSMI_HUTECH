using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Models;

public partial class StaffDepart
{
    public string StaffId { get; set; } = null!;

    public string DepartId { get; set; } = null!;

    public virtual DepartmentStaff Staff { get; set; }

    public virtual Department Department { get; set; }
}
