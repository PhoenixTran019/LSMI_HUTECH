using System;
using System.Collections.Generic;

namespace LmsMini.Domain.Domain.Entities;

public partial class InternAppoval
{
    public string ApprovalId { get; set; } = null!;

    public string? RegistId { get; set; }

    public string? ApproverId { get; set; }

    public string? Decision { get; set; }

    public string? Comments { get; set; }

    public DateTime? ApporvalDate { get; set; }

    public virtual DepartmentStaff? Approver { get; set; }

    public virtual InternRegist? Regist { get; set; }
}
