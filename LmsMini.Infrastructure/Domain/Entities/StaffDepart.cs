using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LmsMini.Infrastructure.Domain.Entities
{
    public class StaffDepart
    {
        public string StaffID { get; set; }
        public string DepartID { get; set; }

        public virtual DepartmentStaff Staff { get; set; }
        public virtual Department Department { get; set; }

    }
}
