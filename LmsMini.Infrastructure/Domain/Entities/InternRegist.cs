using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class InternRegist
{
    public string RegistId { get; set; } = null!;

    public string? IntershipId { get; set; }

    public string? StudentId { get; set; }

    public bool? HasCompany { get; set; }

    public bool? RequestLetter { get; set; }

    public string? Status { get; set; }

    public string? ApprovalNote { get; set; }

    public DateTime? CreateDate { get; set; }

    public virtual ICollection<InternAppoval> InternAppovals { get; set; } = new List<InternAppoval>();

    public virtual ICollection<InternAssignment> InternAssignments { get; set; } = new List<InternAssignment>();

    public virtual ICollection<InternshipCompanyInfor> InternshipCompanyInfors { get; set; } = new List<InternshipCompanyInfor>();

    public virtual Internship? Intership { get; set; }

    public virtual Student? Student { get; set; }
}
