using System;
using System.Collections.Generic;

namespace LmsMini.Infrastructure.Domain.Entities;

public partial class InternshipCompanyInfor
{
    public string CompanyInfoId { get; set; } = null!;

    public string? RegisId { get; set; }

    public string? CompanyName { get; set; }

    public string? Address { get; set; }

    public string? HrrcompanyPhone { get; set; }

    public string? HrrcompanyEmail { get; set; }

    public string? ContactPerson { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactEmployeeCode { get; set; }

    public virtual InternRegist? Regis { get; set; }
}
